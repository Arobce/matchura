using SharedKernel.Events;

namespace Shared.TestUtilities.Fakes;

public class FakeEventBus : IEventBus
{
    private readonly List<object> _publishedEvents = [];

    public IReadOnlyList<object> PublishedEvents => _publishedEvents;

    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        _publishedEvents.Add(@event);
        return Task.CompletedTask;
    }

    public List<T> GetEvents<T>() where T : class
        => _publishedEvents.OfType<T>().ToList();

    public void Clear() => _publishedEvents.Clear();
}
