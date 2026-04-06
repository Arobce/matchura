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

    public async Task SendVerificationCodeAsync(string email, string code)
    {
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                <h2>Verify your email</h2>
                <p>Welcome to Matchura! Enter this code to verify your email address:</p>
                <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px;
                            padding: 16px; background: #f4f4f5; border-radius: 8px;
                            text-align: center; margin: 24px 0;">
                    {code}
                </div>
                <p>This code expires in 15 minutes.</p>
                <p style="color: #71717a; font-size: 14px;">If you didn't create a Matchura account, ignore this email.</p>
            </div>
            """;

        await SendEmailAsync(email, "Matchura - Verify your email", html, code, "Verification");
    }

    public async Task SendTwoFactorCodeAsync(string email, string code)
    {
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                <h2>Login verification</h2>
                <p>Your Matchura login verification code is:</p>
                <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px;
                            padding: 16px; background: #f4f4f5; border-radius: 8px;
                            text-align: center; margin: 24px 0;">
                    {code}
                </div>
                <p>This code expires in 10 minutes.</p>
                <p style="color: #71717a; font-size: 14px;">If you didn't request this code, ignore this email.</p>
            </div>
            """;

        await SendEmailAsync(email, "Matchura - Login verification code", html, code, "2FA");
    }

    private async Task SendEmailAsync(string email, string subject, string html, string code, string label)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@matchura.dev";
        var fromName = _configuration["Smtp:FromName"] ?? "Matchura";

        if (string.IsNullOrEmpty(smtpHost))
        {
            _logger.LogWarning("SMTP not configured. {Label} code for {Email}: {Code}", label, email, code);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = html };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

        if (!string.IsNullOrEmpty(smtpUser))
            await client.AuthenticateAsync(smtpUser, smtpPass);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("{Label} code sent to {Email}", label, email);
    }
}
