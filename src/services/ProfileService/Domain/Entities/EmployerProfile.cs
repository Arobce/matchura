namespace ProfileService.Domain.Entities;

public class EmployerProfile
{
    public Guid EmployerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CompanyLocation { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
