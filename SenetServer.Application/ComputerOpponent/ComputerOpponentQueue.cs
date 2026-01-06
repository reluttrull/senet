using SenetServer.Model;
using System.Threading.Channels;

namespace SenetServer.Application.ComputerOpponent
{
    public class ComputerOpponentQueue : IComputerOpponentQueue
    {
        private readonly Channel<GameState> _queue;

        public ComputerOpponentQueue()
        {
            // bounded channel to prevent excessive memory usage
            var options = new BoundedChannelOptions(capacity: 100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<GameState>(options);
        }
        public async ValueTask EnqueueAsync(GameState gameState) =>
                    await _queue.Writer.WriteAsync(gameState);

        public async ValueTask<GameState> DequeueAsync(CancellationToken cancellationToken) =>
            await _queue.Reader.ReadAsync(cancellationToken);
    }
}
