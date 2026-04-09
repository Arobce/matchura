using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
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
        var apiKey = _configuration["SendLayer:ApiKey"];
        var fromEmail = _configuration["SendLayer:FromEmail"] ?? "REDACTED";
        var fromName = _configuration["SendLayer:FromName"] ?? "Matchura";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SendLayer not configured. {Label} code for {Email}: {Code}", label, email, code);
            return;
        }

        var payload = new SendLayerRequest
        {
            From = new EmailAddress { Name = fromName, Email = fromEmail },
            To = [new EmailAddress { Name = email, Email = email }],
            Subject = subject,
            ContentType = "HTML",
            HtmlContent = html,
        };

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://console.sendlayer.com/api/v1/email");
            request.Headers.Add("Authorization", "Bearer " + apiKey);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("{Label} code sent to {Email}", label, email);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogWarning("SendLayer returned {Status} for {Email}. {Label} code: {Code}. Response: {Body}",
                    response.StatusCode, email, label, code, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email send failed for {Email}. {Label} code: {Code}", email, label, code);
        }
    }

    private class SendLayerRequest
    {
        public EmailAddress From { get; set; } = default!;
        public List<EmailAddress> To { get; set; } = [];
        public string Subject { get; set; } = string.Empty;
        public string ContentType { get; set; } = "HTML";

        [JsonPropertyName("HTMLContent")]
        public string HtmlContent { get; set; } = string.Empty;
    }

    private class EmailAddress
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Email")]
        public string Email { get; set; } = string.Empty;
    }
}
