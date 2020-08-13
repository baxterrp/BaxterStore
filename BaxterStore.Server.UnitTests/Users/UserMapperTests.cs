using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Implementation.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using NUnit.Framework;

namespace BaxterStore.Server.UnitTests.Users
{
    [TestFixture]
    public class UserMapperTests
    {
        private static IMapper<User, UserDataEntity> _mapper;
        private static readonly string _testId = "1d7d0504-a626-4eb1-80ca-efb14e47730a";

        public UserMapperTests()
        {
            _mapper = new UserMapper();
        }

        [Test]
        public void MapToEntityMapsCorrectly()
        {
            var user = GetUser();

            var result = _mapper.MapToEntity(user);

            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.FirstName, result.FirstName);
            Assert.AreEqual(user.LastName, result.LastName);
            Assert.AreEqual(user.Email, result.Email);
            Assert.AreEqual(user.Password, result.Password);
        }

        [Test]
        public void MapToResourceMapsCorrectly()
        {
            var userDataEntity = GetUserDataEntity();

            var result = _mapper.MapToResource(userDataEntity);

            Assert.AreEqual(userDataEntity.Id, result.Id);
            Assert.AreEqual(userDataEntity.FirstName, result.FirstName);
            Assert.AreEqual(userDataEntity.LastName, result.LastName);
            Assert.AreEqual(userDataEntity.Email, result.Email);
            Assert.AreEqual(userDataEntity.Password, result.Password);
        }

        private UserDataEntity GetUserDataEntity() => new UserDataEntity
        {
            Id = _testId,
            FirstName = "Rob",
            LastName = "Baxter",
            Email = "testEmail@test.com",
            Password = "somereallygoodpassword"
        };

        private User GetUser() => new User
        {
            Id = _testId,
            FirstName = "Rob",
            LastName = "Baxter",
            Email = "testEmail@test.com",
            Password = "somereallygoodpassword"
        };
    }
}
