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
        public ValueTask<GameState> DequeueAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask EnqueueAsync(GameState gameState)
        {
            throw new NotImplementedException();
        }
    }
}
