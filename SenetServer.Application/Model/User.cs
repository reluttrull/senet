namespace SenetServer.Model
{
    public class User
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsComputer => UserName == Constants.ComputerOpponentName;

        public User(string userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
    }
}
