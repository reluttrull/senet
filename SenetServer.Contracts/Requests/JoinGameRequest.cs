using System;
using System.Collections.Generic;
using System.Text;

namespace SenetServer.Contracts.Requests
{
    public class StartGameRequest
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
    }
}
