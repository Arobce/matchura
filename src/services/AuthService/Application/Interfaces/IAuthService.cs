using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);
    Task ResendVerificationAsync(string email);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GetCurrentUserAsync(string userId);
    Task<AuthResponse> VerifyTwoFactorAsync(TwoFactorRequest request);
    Task Toggle2FAAsync(string userId, bool enable);
}
