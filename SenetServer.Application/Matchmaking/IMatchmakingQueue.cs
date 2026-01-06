using SenetServer.Model;

namespace SenetServer.Matchmaking
{
    public interface IMatchmakingQueue
    {
        ValueTask EnqueueAsync(MatchRequest request);
        ValueTask<MatchRequest> DequeueAsync(CancellationToken cancellationToken);
        bool TryPeek(out MatchRequest request);
        int Count { get; }
    }
}
