using SenetServer.Model;

namespace SenetServer.Application.ComputerOpponent
{
    public interface IComputerOpponentQueue
    {
        ValueTask EnqueueAsync(GameState gameState);
        ValueTask<GameState> DequeueAsync(CancellationToken cancellationToken);
    }
}
