using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;

namespace BaxterStore.Data.Implementation.Users
{
    public class UserRepository : CrudRepository<UserDataEntity>, IUserRepository 
    {
        public UserRepository(DatabaseConfiguration databaseConfiguration, TableConfiguration tableConfiguration) 
            : base(databaseConfiguration, tableConfiguration) { }
    }
}
