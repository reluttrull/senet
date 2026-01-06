namespace SenetServer.Model
{
    public class MatchResponse
    {
        public User PlayerWhite { get; set; }
        public User PlayerBlack { get; set; }
        public DateTime TimeMatched { get; set; }
    }
}
