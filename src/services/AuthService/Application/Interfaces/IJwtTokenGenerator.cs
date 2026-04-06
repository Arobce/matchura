using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user, string role);
}
