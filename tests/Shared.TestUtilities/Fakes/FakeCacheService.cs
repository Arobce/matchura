using System.Text.Json;
using AIService.Application.Interfaces;

namespace Shared.TestUtilities.Fakes;

public class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, string> _cache = new();

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var json))
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        _cache[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
