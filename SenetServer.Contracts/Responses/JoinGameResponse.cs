namespace SenetServer.Contracts.Responses
{
    public class JoinGameResponse
    {
        public required string UserId { get; init; }
        public required string UserName { get; init; }
    }
}
