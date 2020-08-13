using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;

namespace BaxterStore.Data.Implementation.Users
{
    public class UserRepository : CrudRepository<UserDataEntity> 
    {
        public UserRepository(DatabaseConfiguration databaseConfiguration, TableConfiguration tableConfiguration, ICacheHandler<UserDataEntity> cacheHandler) 
            : base(databaseConfiguration, tableConfiguration, cacheHandler) { }
    }
}
