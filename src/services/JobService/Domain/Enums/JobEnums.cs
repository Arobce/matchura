namespace JobService.Domain.Enums;

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Internship,
    Remote
}

public enum JobStatus
{
    Draft,
    Active,
    Closed,
    Expired
}

public enum ImportanceLevel
{
    Required,
    Preferred,
    NiceToHave
}
