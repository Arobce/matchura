using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using AIService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Events;

namespace AIService.Infrastructure.Services;

public class JobMatchingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JobMatchingWorker> _logger;
    private readonly IEventBus _eventBus;
    private const string ExchangeName = "matchura.events";
    private const decimal MatchThreshold = 70m;
    private const double SkillOverlapThreshold = 0.3;

    public JobMatchingWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<JobMatchingWorker> logger,
        IEventBus eventBus)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
        _eventBus = eventBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = _configuration["RABBITMQ_USER"] ?? "matchura",
            Password = _configuration["RABBITMQ_PASS"] ?? "matchura_dev"
        };

        IConnection? connection = null;
        IChannel? channel = null;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt} failed, retrying...", attempt + 1);
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (connection == null || channel == null)
        {
            _logger.LogError("Failed to connect to RabbitMQ after 10 attempts");
            return;
        }

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        var queueResult = await channel.QueueDeclareAsync("ai-job-matching", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queueResult.QueueName, ExchangeName, "JobPublishedEvent", cancellationToken: stoppingToken);

        // Process one job at a time to avoid overwhelming the Claude API
        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<JobPublishedEvent>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (evt != null)
                {
                    await ProcessJobPublishedAsync(evt, stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing JobPublishedEvent");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(queueResult.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        _logger.LogInformation("JobMatchingWorker started, listening for JobPublishedEvent");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (channel != null) await channel.CloseAsync();
            if (connection != null) await connection.CloseAsync();
        }
    }

    private async Task ProcessJobPublishedAsync(JobPublishedEvent evt, CancellationToken ct)
    {
        _logger.LogInformation("Processing auto-match for job {JobId}: {Title}", evt.JobId, evt.Title);

        // Fetch job skills from JobService API
        var jobSkillNames = await FetchJobSkillNamesAsync(evt.JobId);
        if (jobSkillNames.Count == 0)
        {
            _logger.LogWarning("Job {JobId} has no skills defined, skipping auto-match", evt.JobId);
            return;
        }

        // Tier 1: Get all candidates with parsed resumes and pre-filter by skill overlap
        List<string> filteredCandidateIds;
        int totalCandidates;

        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();

            // Get all distinct candidate IDs that have a parsed resume
            var candidateIds = await db.Resumes
                .Where(r => r.ParsedData != null)
                .Select(r => r.CandidateId)
                .Distinct()
                .ToListAsync(ct);

            totalCandidates = candidateIds.Count;

            // Get all candidate skills in one query
            var allCandidateSkills = await db.CandidateSkills
                .Where(cs => candidateIds.Contains(cs.CandidateId))
                .Select(cs => new { cs.CandidateId, cs.SkillName })
                .ToListAsync(ct);

            var skillsByCandidate = allCandidateSkills
                .GroupBy(cs => cs.CandidateId)
                .ToDictionary(g => g.Key, g => g.Select(cs => cs.SkillName.ToLowerInvariant()).ToHashSet());

            var jobSkillNamesLower = jobSkillNames.Select(s => s.ToLowerInvariant()).ToHashSet();

            filteredCandidateIds = candidateIds
                .Where(candidateId =>
                {
                    if (!skillsByCandidate.TryGetValue(candidateId, out var candidateSkills))
                        return false;

                    var overlap = candidateSkills.Count(s => jobSkillNamesLower.Contains(s));
                    var overlapRatio = (double)overlap / jobSkillNamesLower.Count;
                    return overlapRatio >= SkillOverlapThreshold;
                })
                .ToList();
        }

        _logger.LogInformation(
            "Job {JobId} Tier 1: {Filtered}/{Total} candidates passed skill overlap filter (≥{Threshold}%)",
            evt.JobId, filteredCandidateIds.Count, totalCandidates, SkillOverlapThreshold * 100);

        if (filteredCandidateIds.Count == 0) return;

        // Tier 2: Run AI matching for filtered candidates
        int matchedCount = 0;

        foreach (var candidateId in filteredCandidateIds)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();

                var result = await matchingService.ComputeMatchAsync(candidateId, new ComputeMatchRequest { JobId = evt.JobId });

                if (result.OverallScore >= MatchThreshold)
                {
                    matchedCount++;
                    await _eventBus.PublishAsync(new JobMatchedEvent
                    {
                        JobId = evt.JobId,
                        JobTitle = evt.Title,
                        CandidateId = candidateId,
                        MatchScore = result.OverallScore,
                        OccurredAt = DateTime.UtcNow
                    }, ct);

                    _logger.LogInformation(
                        "Job {JobId}: candidate {CandidateId} matched at {Score}%",
                        evt.JobId, candidateId, result.OverallScore);
                }

                // Rate limit: wait between Claude API calls
                await Task.Delay(500, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to compute match for candidate {CandidateId} on job {JobId}",
                    candidateId, evt.JobId);
            }
        }

        _logger.LogInformation(
            "Job {JobId} auto-match complete: {Matched}/{Filtered} candidates matched ≥{Threshold}% (from {Total} total)",
            evt.JobId, matchedCount, filteredCandidateIds.Count, MatchThreshold, totalCandidates);
    }

    private async Task<List<string>> FetchJobSkillNamesAsync(Guid jobId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient();

            var response = await client.GetAsync($"{jobServiceUrl}/api/jobs/{jobId}");
            if (!response.IsSuccessStatusCode) return new();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            if (!doc.RootElement.TryGetProperty("skills", out var skillsElement))
                return new();

            var skillNames = new List<string>();
            foreach (var skill in skillsElement.EnumerateArray())
            {
                if (skill.TryGetProperty("skillName", out var nameEl) && nameEl.GetString() is { } name)
                    skillNames.Add(name);
            }
            return skillNames;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch skills for job {JobId}", jobId);
            return new();
        }
    }
}
