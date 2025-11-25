namespace SenetServer.Model
{
    public class GameStart
    {
        public string OpponentName { get; set; }
        public List<int> WhitePositions { get; set; }
        public List<int> BlackPositions { get; set; }
        public List<int> Sticks { get; set; }
        public int SticksValue { get; set; }
        public bool IsPlayerWhite { get; set; }
        public bool IsPlayerTurn { get; set; }

        public GameStart(GameState startingGameState, bool isPlayerWhite)
        {
            if (isPlayerWhite) OpponentName = startingGameState.PlayerBlack.UserName;
            else OpponentName = startingGameState.PlayerWhite.UserName;
            WhitePositions = startingGameState.WhitePositions;
            BlackPositions = startingGameState.BlackPositions;
            Sticks = startingGameState.Sticks;
            SticksValue = startingGameState.GetSticksValue();
            IsPlayerWhite = isPlayerWhite;
            IsPlayerTurn = isPlayerWhite ? startingGameState.IsWhiteTurn : !startingGameState.IsWhiteTurn;
        }
    }
}
