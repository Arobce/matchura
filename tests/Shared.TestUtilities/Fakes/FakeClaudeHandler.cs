using System.Net;
using System.Text;
using System.Text.Json;

namespace Shared.TestUtilities.Fakes;

/// <summary>
/// A delegating handler that intercepts HTTP calls to the Claude API and returns
/// configurable fake responses. The handler wraps inner JSON payloads in the
/// standard Claude API response envelope:
/// <code>{ "content": [{ "type": "text", "text": "..." }] }</code>
/// </summary>
public class FakeClaudeHandler : DelegatingHandler
{
    private readonly Queue<(HttpStatusCode Status, string? Body)> _responses = new();

    public FakeClaudeHandler() : base(new HttpClientHandler()) { }

    /// <summary>
    /// Enqueue a successful response whose text payload is the given JSON string.
    /// The string is automatically wrapped in the Claude API envelope.
    /// </summary>
    public void EnqueueSuccess(string innerJson)
    {
        var envelope = WrapInEnvelope(innerJson);
        _responses.Enqueue((HttpStatusCode.OK, envelope));
    }

    /// <summary>
    /// Enqueue a raw HTTP response with the given status code and optional body.
    /// No envelope wrapping is applied -- the body is sent as-is.
    /// </summary>
    public void EnqueueRaw(HttpStatusCode statusCode, string? body = null)
    {
        _responses.Enqueue((statusCode, body));
    }

    /// <summary>
    /// Enqueue a successful response whose text payload is malformed (not valid JSON).
    /// This is wrapped in the Claude API envelope so the outer parse succeeds but
    /// inner deserialization fails.
    /// </summary>
    public void EnqueueMalformedJson()
    {
        var envelope = WrapInEnvelope("this is not valid json {{{");
        _responses.Enqueue((HttpStatusCode.OK, envelope));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_responses.Count == 0)
            throw new InvalidOperationException(
                "FakeClaudeHandler: no more enqueued responses. Did you forget to enqueue?");

        var (status, body) = _responses.Dequeue();
        var response = new HttpResponseMessage(status);
        if (body != null)
            response.Content = new StringContent(body, Encoding.UTF8, "application/json");

        return Task.FromResult(response);
    }

    private static string WrapInEnvelope(string textPayload)
    {
        var envelope = new
        {
            id = "msg_fake_test",
            type = "message",
            role = "assistant",
            content = new[]
            {
                new { type = "text", text = textPayload }
            },
            model = "claude-sonnet-4-20250514",
            stop_reason = "end_turn",
            usage = new { input_tokens = 100, output_tokens = 50 }
        };
        return JsonSerializer.Serialize(envelope);
    }
}
