using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace AIService.Agents.Core;

public class ClaudeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ClaudeApiClient(HttpClient httpClient, ILogger<ClaudeApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(string systemPrompt, string userMessage, CancellationToken ct = default)
    {
        var requestBody = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 4096,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);

        var retryCount = 0;
        const int maxRetries = 3;

        while (true)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("v1/messages", content, ct);

                if (response.StatusCode == HttpStatusCode.TooManyRequests
                    || (int)response.StatusCode == 529)
                {
                    retryCount++;
                    if (retryCount > maxRetries)
                        throw new InvalidOperationException("Claude API rate limit exceeded after retries");

                    var retryAfter = response.Headers.RetryAfter?.Delta;
                    var delay = retryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, retryCount + 1));
                    _logger.LogWarning("Claude API overloaded ({StatusCode}), retrying in {Delay}s (attempt {Attempt}/{Max})",
                        (int)response.StatusCode, delay.TotalSeconds, retryCount, maxRetries);
                    await Task.Delay(delay, ct);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync(ct);

                using var doc = JsonDocument.Parse(responseBody);
                var contentArray = doc.RootElement.GetProperty("content");
                foreach (var block in contentArray.EnumerateArray())
                {
                    if (block.GetProperty("type").GetString() == "text")
                        return block.GetProperty("text").GetString() ?? "";
                }

                throw new InvalidOperationException("No text content in Claude response");
            }
            catch (HttpRequestException ex) when (retryCount < maxRetries)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogWarning(ex, "Claude API request failed, retrying in {Delay}s (attempt {Attempt}/{Max})",
                    delay.TotalSeconds, retryCount, maxRetries);
                await Task.Delay(delay, ct);
            }
        }
    }

    public async Task<T> SendAndParseAsync<T>(string systemPrompt, string userMessage, CancellationToken ct = default)
    {
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var rawResponse = await SendMessageAsync(systemPrompt, userMessage, ct);

            // Extract JSON from response (handle markdown code blocks)
            var jsonText = ExtractJson(rawResponse);

            try
            {
                var result = JsonSerializer.Deserialize<T>(jsonText, JsonOptions);
                if (result == null)
                    throw new JsonException("Deserialized to null");
                return result;
            }
            catch (JsonException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning("Agent self-correction: attempt {Attempt} failed to parse JSON: {Error}",
                    attempt, ex.Message);

                // Self-correction: ask Claude to fix its output
                userMessage = $"""
                    Your previous response was not valid JSON. The parsing error was:
                    {ex.Message}

                    Your response was:
                    {rawResponse}

                    Please return ONLY valid JSON matching the exact schema requested. No markdown, no explanations, just the JSON object.
                    """;
            }
        }

        throw new InvalidOperationException($"Agent failed to produce valid JSON after {maxAttempts} attempts");
    }

    private static string ExtractJson(string text)
    {
        text = text.Trim();

        // Remove markdown code blocks
        if (text.StartsWith("```json"))
            text = text[7..];
        else if (text.StartsWith("```"))
            text = text[3..];

        if (text.EndsWith("```"))
            text = text[..^3];

        text = text.Trim();

        // Find the JSON object/array boundaries
        var start = text.IndexOfAny(['{', '[']);
        if (start < 0) return text;

        var end = text.LastIndexOfAny(['}', ']']);
        if (end < 0) return text;

        return text[start..(end + 1)];
    }
}
