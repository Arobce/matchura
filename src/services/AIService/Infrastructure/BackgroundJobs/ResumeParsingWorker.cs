using System.Text.Json;
using System.Threading.Channels;
using AIService.Agents;
using AIService.Domain.Entities;
using AIService.Domain.Enums;
using AIService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIService.Infrastructure.BackgroundJobs;

public class ResumeParsingWorker : BackgroundService
{
    private readonly Channel<Guid> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ResumeParsingWorker> _logger;

    public ResumeParsingWorker(
        Channel<Guid> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<ResumeParsingWorker> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Resume parsing worker started");

        await foreach (var resumeId in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessResumeAsync(resumeId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process resume {ResumeId}", resumeId);
            }
        }
    }

    private async Task ProcessResumeAsync(Guid resumeId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        var agent = scope.ServiceProvider.GetRequiredService<ResumeParserAgent>();

        var resume = await db.Resumes.FirstOrDefaultAsync(r => r.ResumeId == resumeId, ct);
        if (resume == null)
        {
            _logger.LogWarning("Resume {ResumeId} not found", resumeId);
            return;
        }

        if (string.IsNullOrWhiteSpace(resume.RawText))
        {
            resume.ParseStatus = ParseStatus.Failed;
            resume.ErrorMessage = "No text content extracted from resume";
            await db.SaveChangesAsync(ct);
            return;
        }

        try
        {
            resume.ParseStatus = ParseStatus.Parsing;
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Parsing resume {ResumeId} for candidate {CandidateId}",
                resumeId, resume.CandidateId);

            var parsedData = await agent.ParseAsync(resume.RawText, ct);

            resume.ParsedData = JsonSerializer.Serialize(parsedData);
            resume.ParseStatus = ParseStatus.Completed;
            resume.ParsedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            // Populate CandidateSkills from parsed data
            await PopulateCandidateSkillsAsync(db, resume.CandidateId, parsedData, ct);

            _logger.LogInformation("Resume {ResumeId} parsed successfully — {SkillCount} skills extracted",
                resumeId, parsedData.Skills.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resume parsing failed for {ResumeId}", resumeId);
            resume.ParseStatus = ParseStatus.Failed;
            resume.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task PopulateCandidateSkillsAsync(
        AIDbContext db, string candidateId,
        Application.DTOs.ParsedResumeData parsedData, CancellationToken ct)
    {
        // Remove old parsed skills for this candidate
        var oldSkills = await db.CandidateSkills
            .Where(s => s.CandidateId == candidateId && s.Source == "resume_parse")
            .ToListAsync(ct);
        db.CandidateSkills.RemoveRange(oldSkills);

        foreach (var skill in parsedData.Skills)
        {
            var proficiency = Enum.TryParse<ProficiencyLevel>(skill.ProficiencyLevel, true, out var p)
                ? p : ProficiencyLevel.Intermediate;

            db.CandidateSkills.Add(new CandidateSkill
            {
                CandidateId = candidateId,
                SkillName = skill.Name,
                SkillCategory = skill.Category,
                ProficiencyLevel = proficiency,
                YearsUsed = skill.YearsUsed,
                Source = "resume_parse"
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
