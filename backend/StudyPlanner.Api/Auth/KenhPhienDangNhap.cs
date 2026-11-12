using System.Collections.Concurrent;
using System.Threading.Channels;

namespace StudyPlanner.Api.Auth;

// One subscription per open application tab. No token is retained in this registry.
public sealed class KenhPhienDangNhap
{
    private readonly ConcurrentDictionary<Guid, DangKy> subscriptions = new();

    public DangKy Mo(int userId)
    {
        var subscription = new DangKy(this, userId);
        subscriptions[subscription.Id] = subscription;
        return subscription;
    }

    public void Phat(int userId, string eventName)
    {
        foreach (var subscription in subscriptions.Values)
            if (subscription.UserId == userId) subscription.Channel.Writer.TryWrite(eventName);
    }

    public sealed class DangKy : IDisposable
    {
        private readonly KenhPhienDangNhap owner;
        internal Guid Id { get; } = Guid.NewGuid();
        internal int UserId { get; }
        internal Channel<string> Channel { get; } = System.Threading.Channels.Channel.CreateBounded<string>(
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
        public ChannelReader<string> Reader => Channel.Reader;
        internal DangKy(KenhPhienDangNhap owner, int userId) { this.owner = owner; UserId = userId; }
        public void Dispose()
        {
            owner.subscriptions.TryRemove(Id, out _);
            Channel.Writer.TryComplete();
        }
    }
}
