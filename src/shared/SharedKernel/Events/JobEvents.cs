namespace SharedKernel.Events;

public record JobPublishedEvent
{
    public Guid JobId { get; init; }
    public string EmployerId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}

public record JobMatchedEvent
{
    public Guid JobId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public decimal MatchScore { get; init; }
    public DateTime OccurredAt { get; init; }
}
