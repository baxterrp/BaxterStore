using BaxterStore.Data.POCOs;
using BaxterStore.Service.POCOs;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaxterStore.Service.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterNewUser(User user);
        Task<User> Login(string email, string password);
        Task<User> UpdateUser(User user);
    }
}
