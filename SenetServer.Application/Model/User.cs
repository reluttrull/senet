namespace SenetServer.Model
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsComputer => UserName == Constants.ComputerOpponentName;

        public User()
        {

        }

        public User(string userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
    }
}
