using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;

namespace BaxterStore.Service.Implementation.Users
{
    public class UserMapper : IMapper<User, UserDataEntity>
    {
        public UserDataEntity MapToEntity(User resource) => new UserDataEntity
        {
            Id = resource.Id,
            Email = resource.Email,
            Password = resource.Password,
            FirstName = resource.FirstName,
            LastName = resource.LastName
        };

        public User MapToResource(UserDataEntity dataEntity) => new User
        {
            Id = dataEntity.Id,
            Email = dataEntity.Email,
            Password = dataEntity.Password,
            FirstName = dataEntity.FirstName,
            LastName = dataEntity.LastName
        };
    }
}
