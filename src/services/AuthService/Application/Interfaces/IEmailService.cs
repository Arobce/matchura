namespace AuthService.Application.Interfaces;

public interface IEmailService
{
    Task SendTwoFactorCodeAsync(string email, string code);
}
