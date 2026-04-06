namespace AuthService.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code);
    Task SendTwoFactorCodeAsync(string email, string code);
}
