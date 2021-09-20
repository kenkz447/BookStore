using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data.ViewModels.Authentication
{
    public class AuthResponseVM
    {
    }

    public class RoleToUser
    {
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
