using AuthService.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool TwoFactorEmailEnabled { get; set; }
    public string? TwoFactorEmailCode { get; set; }
    public DateTime? TwoFactorEmailCodeExpiry { get; set; }
}
