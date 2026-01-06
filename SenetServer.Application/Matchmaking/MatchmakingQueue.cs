using System.Threading.Channels;
using SenetServer.Model;

namespace SenetServer.Matchmaking
{
    public class MatchmakingQueue : IMatchmakingQueue
    {
        private readonly Channel<MatchRequest> _queue;

        public MatchmakingQueue()
        {
            // bounded channel to prevent excessive memory usage
            var options = new BoundedChannelOptions(capacity: 100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<MatchRequest>(options);
        }

        public async ValueTask EnqueueAsync(MatchRequest request) =>
            await _queue.Writer.WriteAsync(request);

        public async ValueTask<MatchRequest> DequeueAsync(CancellationToken cancellationToken) =>
            await _queue.Reader.ReadAsync(cancellationToken);

        public bool TryPeek(out MatchRequest request) =>
            _queue.Reader.TryPeek(out request);

        public int Count => _queue.Reader.Count;
    }
}
