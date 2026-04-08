namespace SharedKernel.Events;

public record ApplicationSubmittedEvent
{
    public Guid ApplicationId { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public Guid JobId { get; init; }
    public DateTime OccurredAt { get; init; }
}

public record ApplicationStatusChangedEvent
{
    public Guid ApplicationId { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public Guid JobId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}

public record ApplicationWithdrawnEvent
{
    public Guid ApplicationId { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public Guid JobId { get; init; }
    public DateTime OccurredAt { get; init; }
}
