namespace ProfileService.Application.DTOs;

public class CreateEmployerProfileRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanyLocation { get; set; }
    public string? LogoUrl { get; set; }
}

public class UpdateEmployerProfileRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanyLocation { get; set; }
    public string? LogoUrl { get; set; }
}

public class EmployerProfileResponse
{
    public Guid EmployerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanyLocation { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EmployerProfilePublicResponse
{
    public Guid EmployerId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanyLocation { get; set; }
    public string? LogoUrl { get; set; }
}
