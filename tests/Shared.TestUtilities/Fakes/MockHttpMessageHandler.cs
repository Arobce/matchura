using System.Net;

namespace Shared.TestUtilities.Fakes;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();
    public List<HttpRequestMessage> SentRequests { get; } = [];

    public void EnqueueResponse(HttpStatusCode statusCode, string? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
            response.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        _responses.Enqueue(response);
    }

    public void EnqueueJsonResponse<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        EnqueueResponse(statusCode, json);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SentRequests.Add(request);
        if (_responses.Count > 0)
            return Task.FromResult(_responses.Dequeue());
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
