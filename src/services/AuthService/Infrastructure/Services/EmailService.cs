using AuthService.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AuthService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTwoFactorCodeAsync(string email, string code)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@matchura.dev";
        var fromName = _configuration["Smtp:FromName"] ?? "Matchura";

        if (string.IsNullOrEmpty(smtpHost))
        {
            _logger.LogWarning("SMTP not configured. 2FA code for {Email}: {Code}", email, code);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Matchura - Your verification code";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                    <h2>Verification Code</h2>
                    <p>Your Matchura login verification code is:</p>
                    <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px;
                                padding: 16px; background: #f4f4f5; border-radius: 8px;
                                text-align: center; margin: 24px 0;">
                        {code}
                    </div>
                    <p>This code expires in 10 minutes.</p>
                    <p style="color: #71717a; font-size: 14px;">If you didn't request this code, ignore this email.</p>
                </div>
                """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

        if (!string.IsNullOrEmpty(smtpUser))
            await client.AuthenticateAsync(smtpUser, smtpPass);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("2FA code sent to {Email}", email);
    }
}
