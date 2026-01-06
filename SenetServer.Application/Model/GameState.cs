namespace SenetServer.Model
{
    public class GameState
    {
        public User PlayerWhite {  get; set; }
        public User PlayerBlack { get; set; }
        public BoardState BoardState { get; set; }
        public GameState(User user1, User user2)
        {
            PlayerWhite = user1;
            PlayerBlack = user2;
            BoardState = new BoardState();
        }
    }
}
