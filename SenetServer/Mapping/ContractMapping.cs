using SenetServer.Model;
using SenetServer.Contracts.Responses;
using SenetServer.Contracts.Requests;

namespace SenetServer.Mapping
{
    public static class ContractMapping
    {
        public static JoinGameResponse MapToResponse(this User user)
        {
            return new JoinGameResponse()
            { 
                UserId = user.UserId,
                UserName = user.UserName
            };
        }

        public static User MapToUser(this StartGameRequest request)
        {
            return new User()
            {
                UserId = request.UserId,
                UserName = request.UserName
            };
        }
    }
}
