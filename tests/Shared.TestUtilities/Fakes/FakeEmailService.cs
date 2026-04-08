using AuthService.Application.Interfaces;

namespace Shared.TestUtilities.Fakes;

public class FakeEmailService : IEmailService
{
    public List<(string Email, string Code)> SentVerificationCodes { get; } = [];
    public List<(string Email, string Code)> SentTwoFactorCodes { get; } = [];

    public Task SendVerificationCodeAsync(string email, string code)
    {
        SentVerificationCodes.Add((email, code));
        return Task.CompletedTask;
    }

    public Task SendTwoFactorCodeAsync(string email, string code)
    {
        SentTwoFactorCodes.Add((email, code));
        return Task.CompletedTask;
    }
}
