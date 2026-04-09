using System.Text.Json;
using FluentAssertions;
using SharedKernel.Events;

namespace JobService.UnitTests;

public class EventContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    // -----------------------------------------------------------------------
    // JobPublishedEvent
    // -----------------------------------------------------------------------

    [Fact]
    public void JobPublishedEvent_ShouldRoundTripSerialize()
    {
        var original = new JobPublishedEvent
        {
            JobId = Guid.NewGuid(),
            EmployerId = "emp-123",
            Title = "Backend Developer",
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<JobPublishedEvent>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.JobId.Should().Be(original.JobId);
        deserialized.EmployerId.Should().Be(original.EmployerId);
        deserialized.Title.Should().Be(original.Title);
        deserialized.OccurredAt.Should().Be(original.OccurredAt);
    }

    [Fact]
    public void JobPublishedEvent_JsonShouldContainExpectedPropertyNames()
    {
        var evt = new JobPublishedEvent
        {
            JobId = Guid.NewGuid(),
            EmployerId = "emp-1",
            Title = "Dev",
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt, JsonOptions);

        json.Should().Contain("\"jobId\"");
        json.Should().Contain("\"employerId\"");
        json.Should().Contain("\"title\"");
        json.Should().Contain("\"occurredAt\"");
    }

    [Fact]
    public void JobPublishedEvent_ShouldHaveExpectedProperties()
    {
        var type = typeof(JobPublishedEvent);
        type.GetProperty(nameof(JobPublishedEvent.JobId)).Should().NotBeNull();
        type.GetProperty(nameof(JobPublishedEvent.EmployerId)).Should().NotBeNull();
        type.GetProperty(nameof(JobPublishedEvent.Title)).Should().NotBeNull();
        type.GetProperty(nameof(JobPublishedEvent.OccurredAt)).Should().NotBeNull();
        type.GetProperties().Should().HaveCount(4);
    }

    // -----------------------------------------------------------------------
    // JobMatchedEvent
    // -----------------------------------------------------------------------

    [Fact]
    public void JobMatchedEvent_ShouldRoundTripSerialize()
    {
        var original = new JobMatchedEvent
        {
            JobId = Guid.NewGuid(),
            JobTitle = "Frontend Developer",
            CandidateId = "cand-456",
            MatchScore = 0.85m,
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<JobMatchedEvent>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.JobId.Should().Be(original.JobId);
        deserialized.JobTitle.Should().Be(original.JobTitle);
        deserialized.CandidateId.Should().Be(original.CandidateId);
        deserialized.MatchScore.Should().Be(original.MatchScore);
        deserialized.OccurredAt.Should().Be(original.OccurredAt);
    }

    [Fact]
    public void JobMatchedEvent_JsonShouldContainExpectedPropertyNames()
    {
        var evt = new JobMatchedEvent
        {
            JobId = Guid.NewGuid(),
            JobTitle = "Dev",
            CandidateId = "c1",
            MatchScore = 0.9m,
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt, JsonOptions);

        json.Should().Contain("\"jobId\"");
        json.Should().Contain("\"jobTitle\"");
        json.Should().Contain("\"candidateId\"");
        json.Should().Contain("\"matchScore\"");
        json.Should().Contain("\"occurredAt\"");
    }

    [Fact]
    public void JobMatchedEvent_ShouldHaveExpectedProperties()
    {
        var type = typeof(JobMatchedEvent);
        type.GetProperty(nameof(JobMatchedEvent.JobId)).Should().NotBeNull();
        type.GetProperty(nameof(JobMatchedEvent.JobTitle)).Should().NotBeNull();
        type.GetProperty(nameof(JobMatchedEvent.CandidateId)).Should().NotBeNull();
        type.GetProperty(nameof(JobMatchedEvent.MatchScore)).Should().NotBeNull();
        type.GetProperty(nameof(JobMatchedEvent.OccurredAt)).Should().NotBeNull();
        type.GetProperties().Should().HaveCount(5);
    }

    // -----------------------------------------------------------------------
    // ApplicationSubmittedEvent
    // -----------------------------------------------------------------------

    [Fact]
    public void ApplicationSubmittedEvent_ShouldRoundTripSerialize()
    {
        var original = new ApplicationSubmittedEvent
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = "cand-789",
            JobId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ApplicationSubmittedEvent>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.ApplicationId.Should().Be(original.ApplicationId);
        deserialized.CandidateId.Should().Be(original.CandidateId);
        deserialized.JobId.Should().Be(original.JobId);
        deserialized.OccurredAt.Should().Be(original.OccurredAt);
    }

    [Fact]
    public void ApplicationSubmittedEvent_JsonShouldContainExpectedPropertyNames()
    {
        var evt = new ApplicationSubmittedEvent
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = "c1",
            JobId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt, JsonOptions);

        json.Should().Contain("\"applicationId\"");
        json.Should().Contain("\"candidateId\"");
        json.Should().Contain("\"jobId\"");
        json.Should().Contain("\"occurredAt\"");
    }

    [Fact]
    public void ApplicationSubmittedEvent_ShouldHaveExpectedProperties()
    {
        var type = typeof(ApplicationSubmittedEvent);
        type.GetProperty(nameof(ApplicationSubmittedEvent.ApplicationId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationSubmittedEvent.CandidateId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationSubmittedEvent.JobId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationSubmittedEvent.OccurredAt)).Should().NotBeNull();
        type.GetProperties().Should().HaveCount(4);
    }

    // -----------------------------------------------------------------------
    // ApplicationStatusChangedEvent
    // -----------------------------------------------------------------------

    [Fact]
    public void ApplicationStatusChangedEvent_ShouldRoundTripSerialize()
    {
        var original = new ApplicationStatusChangedEvent
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = "cand-101",
            JobId = Guid.NewGuid(),
            OldStatus = "Submitted",
            NewStatus = "Reviewed",
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ApplicationStatusChangedEvent>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.ApplicationId.Should().Be(original.ApplicationId);
        deserialized.CandidateId.Should().Be(original.CandidateId);
        deserialized.JobId.Should().Be(original.JobId);
        deserialized.OldStatus.Should().Be(original.OldStatus);
        deserialized.NewStatus.Should().Be(original.NewStatus);
        deserialized.OccurredAt.Should().Be(original.OccurredAt);
    }

    [Fact]
    public void ApplicationStatusChangedEvent_JsonShouldContainExpectedPropertyNames()
    {
        var evt = new ApplicationStatusChangedEvent
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = "c1",
            JobId = Guid.NewGuid(),
            OldStatus = "A",
            NewStatus = "B",
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt, JsonOptions);

        json.Should().Contain("\"applicationId\"");
        json.Should().Contain("\"candidateId\"");
        json.Should().Contain("\"jobId\"");
        json.Should().Contain("\"oldStatus\"");
        json.Should().Contain("\"newStatus\"");
        json.Should().Contain("\"occurredAt\"");
    }

    [Fact]
    public void ApplicationStatusChangedEvent_ShouldHaveExpectedProperties()
    {
        var type = typeof(ApplicationStatusChangedEvent);
        type.GetProperty(nameof(ApplicationStatusChangedEvent.ApplicationId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationStatusChangedEvent.CandidateId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationStatusChangedEvent.JobId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationStatusChangedEvent.OldStatus)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationStatusChangedEvent.NewStatus)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationStatusChangedEvent.OccurredAt)).Should().NotBeNull();
        type.GetProperties().Should().HaveCount(6);
    }

    // -----------------------------------------------------------------------
    // ApplicationWithdrawnEvent
    // -----------------------------------------------------------------------

    [Fact]
    public void ApplicationWithdrawnEvent_ShouldRoundTripSerialize()
    {
        var original = new ApplicationWithdrawnEvent
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = "cand-202",
            JobId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ApplicationWithdrawnEvent>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.ApplicationId.Should().Be(original.ApplicationId);
        deserialized.CandidateId.Should().Be(original.CandidateId);
        deserialized.JobId.Should().Be(original.JobId);
        deserialized.OccurredAt.Should().Be(original.OccurredAt);
    }

    [Fact]
    public void ApplicationWithdrawnEvent_JsonShouldContainExpectedPropertyNames()
    {
        var evt = new ApplicationWithdrawnEvent
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = "c1",
            JobId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt, JsonOptions);

        json.Should().Contain("\"applicationId\"");
        json.Should().Contain("\"candidateId\"");
        json.Should().Contain("\"jobId\"");
        json.Should().Contain("\"occurredAt\"");
    }

    [Fact]
    public void ApplicationWithdrawnEvent_ShouldHaveExpectedProperties()
    {
        var type = typeof(ApplicationWithdrawnEvent);
        type.GetProperty(nameof(ApplicationWithdrawnEvent.ApplicationId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationWithdrawnEvent.CandidateId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationWithdrawnEvent.JobId)).Should().NotBeNull();
        type.GetProperty(nameof(ApplicationWithdrawnEvent.OccurredAt)).Should().NotBeNull();
        type.GetProperties().Should().HaveCount(4);
    }

    // -----------------------------------------------------------------------
    // Cross-event: all event types are records
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(typeof(JobPublishedEvent))]
    [InlineData(typeof(JobMatchedEvent))]
    [InlineData(typeof(ApplicationSubmittedEvent))]
    [InlineData(typeof(ApplicationStatusChangedEvent))]
    [InlineData(typeof(ApplicationWithdrawnEvent))]
    public void AllEventTypes_ShouldBeRecords(Type eventType)
    {
        // Records in C# have a compiler-generated <Clone>$ method
        var cloneMethod = eventType.GetMethod("<Clone>$");
        cloneMethod.Should().NotBeNull(
            $"{eventType.Name} should be a record type");
    }

    [Theory]
    [InlineData(typeof(JobPublishedEvent))]
    [InlineData(typeof(JobMatchedEvent))]
    [InlineData(typeof(ApplicationSubmittedEvent))]
    [InlineData(typeof(ApplicationStatusChangedEvent))]
    [InlineData(typeof(ApplicationWithdrawnEvent))]
    public void AllEventTypes_ShouldHaveOccurredAtProperty(Type eventType)
    {
        var prop = eventType.GetProperty("OccurredAt");
        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(DateTime));
    }
}
