using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Services;

namespace NotificationService.UnitTests.Workers;

public class RabbitMqConsumerWorkerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly IConfiguration _configuration;
    private readonly RabbitMqConsumerWorker _worker;

    public RabbitMqConsumerWorkerTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JOB_SERVICE_URL"] = "http://localhost:5000"
            })
            .Build();

        var scopeFactory = new Mock<IServiceScopeFactory>();
        var logger = new Mock<ILogger<RabbitMqConsumerWorker>>();

        _worker = new RabbitMqConsumerWorker(scopeFactory.Object, _configuration, logger.Object);
    }

    private void SetupJobInfoResponse(Guid jobId, string title, string employerId)
    {
        var jobJson = JsonSerializer.Serialize(new { title, employerId });
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jobJson, Encoding.UTF8, "application/json")
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
    }

    // ── HandleApplicationSubmitted ──

    [Fact]
    public async Task HandleApplicationSubmitted_CreatesNotificationsForEmployerAndCandidate()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-100";
        var employerId = "employer-200";
        var jobTitle = "Senior Developer";

        SetupJobInfoResponse(jobId, jobTitle, employerId);

        var eventBody = JsonSerializer.Serialize(new
        {
            applicationId,
            candidateId,
            jobId,
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleApplicationSubmitted(
            eventBody, _notificationServiceMock.Object, _httpClientFactoryMock.Object, _configuration);

        // Assert - employer notification
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            employerId,
            "ApplicationSubmitted",
            "New Application",
            It.Is<string>(m => m.Contains(jobTitle)),
            applicationId.ToString(),
            "Application"), Times.Once);

        // Assert - candidate notification
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            candidateId,
            "ApplicationConfirmed",
            "Application Submitted",
            It.Is<string>(m => m.Contains(jobTitle)),
            applicationId.ToString(),
            "Application"), Times.Once);
    }

    [Fact]
    public async Task HandleApplicationSubmitted_NoEmployerId_OnlyCreatesCandidateNotification()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-solo";

        // Return empty employer ID from job service
        var jobJson = JsonSerializer.Serialize(new { title = "Test Job", employerId = (string?)null });
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jobJson, Encoding.UTF8, "application/json")
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var eventBody = JsonSerializer.Serialize(new
        {
            applicationId,
            candidateId,
            jobId,
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleApplicationSubmitted(
            eventBody, _notificationServiceMock.Object, _httpClientFactoryMock.Object, _configuration);

        // Assert - only candidate notification, no employer notification
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            candidateId,
            "ApplicationConfirmed",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);

        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            It.IsAny<string>(),
            "ApplicationSubmitted",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    // ── HandleApplicationStatusChanged ──

    [Theory]
    [InlineData("Reviewed", "Application Reviewed")]
    [InlineData("Shortlisted", "You're Shortlisted!")]
    [InlineData("Accepted", "Application Accepted!")]
    [InlineData("Rejected", "Application Update")]
    public async Task HandleApplicationStatusChanged_CreatesNotificationWithCorrectTitle(
        string newStatus, string expectedTitle)
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-status";
        var jobTitle = "Frontend Engineer";

        SetupJobInfoResponse(jobId, jobTitle, "employer-x");

        var eventBody = JsonSerializer.Serialize(new
        {
            applicationId,
            candidateId,
            jobId,
            oldStatus = "Submitted",
            newStatus,
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleApplicationStatusChanged(
            eventBody, _notificationServiceMock.Object, _httpClientFactoryMock.Object, _configuration);

        // Assert
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            candidateId,
            "ApplicationStatusChanged",
            expectedTitle,
            It.Is<string>(m => m.Contains(jobTitle)),
            applicationId.ToString(),
            "Application"), Times.Once);
    }

    [Fact]
    public async Task HandleApplicationStatusChanged_UnknownStatus_FallsBackToGenericMessage()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-unknown";
        var jobTitle = "Data Analyst";

        SetupJobInfoResponse(jobId, jobTitle, "employer-y");

        var eventBody = JsonSerializer.Serialize(new
        {
            applicationId,
            candidateId,
            jobId,
            oldStatus = "Submitted",
            newStatus = "OnHold",
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleApplicationStatusChanged(
            eventBody, _notificationServiceMock.Object, _httpClientFactoryMock.Object, _configuration);

        // Assert
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            candidateId,
            "ApplicationStatusChanged",
            "Status Update",
            It.Is<string>(m => m.Contains("OnHold") && m.Contains(jobTitle)),
            applicationId.ToString(),
            "Application"), Times.Once);
    }

    // ── HandleApplicationWithdrawn ──

    [Fact]
    public async Task HandleApplicationWithdrawn_CreatesNotificationForEmployer()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-withdraw";
        var employerId = "employer-withdraw";
        var jobTitle = "DevOps Engineer";

        SetupJobInfoResponse(jobId, jobTitle, employerId);

        var eventBody = JsonSerializer.Serialize(new
        {
            applicationId,
            candidateId,
            jobId,
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleApplicationWithdrawn(
            eventBody, _notificationServiceMock.Object, _httpClientFactoryMock.Object, _configuration);

        // Assert
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            employerId,
            "ApplicationWithdrawn",
            "Application Withdrawn",
            It.Is<string>(m => m.Contains(jobTitle)),
            applicationId.ToString(),
            "Application"), Times.Once);
    }

    [Fact]
    public async Task HandleApplicationWithdrawn_NoEmployerId_DoesNotCreateNotification()
    {
        // Arrange - job service returns failure
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var eventBody = JsonSerializer.Serialize(new
        {
            applicationId = Guid.NewGuid(),
            candidateId = "candidate-x",
            jobId = Guid.NewGuid(),
            occurredAt = DateTime.UtcNow
        });

        // Act
        await _worker.HandleApplicationWithdrawn(
            eventBody, _notificationServiceMock.Object, _httpClientFactoryMock.Object, _configuration);

        // Assert - no notifications created since employerId is null
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ── HandleJobMatched ──

    [Fact]
    public async Task HandleJobMatched_CreatesNotificationForCandidateWithScore()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-match";
        var jobTitle = "Backend Developer";
        var matchScore = 87.5m;

        var eventBody = JsonSerializer.Serialize(new
        {
            jobId,
            jobTitle,
            candidateId,
            matchScore,
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleJobMatched(eventBody, _notificationServiceMock.Object);

        // Assert
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            candidateId,
            "JobMatched",
            "New Job Match!",
            It.Is<string>(m => m.Contains("88") && m.Contains(jobTitle)),
            jobId.ToString(),
            "Job"), Times.Once);
    }

    [Fact]
    public async Task HandleJobMatched_HighScore_FormatsPercentageCorrectly()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var candidateId = "candidate-perfect";
        var jobTitle = "Architect";
        var matchScore = 100.0m;

        var eventBody = JsonSerializer.Serialize(new
        {
            jobId,
            jobTitle,
            candidateId,
            matchScore,
            occurredAt = DateTime.UtcNow
        });

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _worker.HandleJobMatched(eventBody, _notificationServiceMock.Object);

        // Assert
        _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
            candidateId,
            "JobMatched",
            "New Job Match!",
            It.Is<string>(m => m.Contains("100") && m.Contains(jobTitle)),
            jobId.ToString(),
            "Job"), Times.Once);
    }
}
