using BaxterStore.Data.Exceptions;
using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Implementation.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using BCrypt;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaxterStore.Server.UnitTests.Users
{
    [TestFixture]
    public class UserServiceTests
    {
        private static Mock<ICrudRepository<UserDataEntity>> _mockUserRepository;
        private static IMapper<User, UserDataEntity> _userMapper;
        private static Mock<ILogger<UserService>> _mockLogger;

        private static readonly string _testId = "1d7d0504-a626-4eb1-80ca-efb14e47730a";
        private static readonly string _testPassword = "12345";
        private static readonly string _testEmail = "test@test.com";

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<ICrudRepository<UserDataEntity>>();
            _userMapper = new UserMapper();
            _mockLogger = new Mock<ILogger<UserService>>();
        }

        #region Login

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("      ")]
        public void LoginThrowsIfEmailInvalid(string email)
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.Login(email, _testPassword));

            Assert.AreEqual("email", exception.ParamName);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("      ")]
        public void LoginThrowsIfPasswordInvalid(string password)
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.Login(_testEmail, password));

            Assert.AreEqual("password", exception.ParamName);
        }

        [Test]
        public void LoginThrowsInvalidLoginAttemptFindUserReturnsNull()
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            _mockUserRepository.Setup(x => x.Search(It.IsAny<IEnumerable<SearchParameter>>())).ReturnsAsync((IEnumerable<UserDataEntity>)null);

            var exception = Assert.ThrowsAsync<InvalidLoginAttemptException>(async () => await sut.Login(_testEmail, _testPassword));

            Assert.AreEqual($"No user found with email {_testEmail}", exception.Message);
        }

        [Test]
        public void LoginThrowsInvalidLoginAttemptIfFindUserReturnsEmpty()
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            _mockUserRepository.Setup(x => x.Search(It.IsAny<IEnumerable<SearchParameter>>())).ReturnsAsync(new List<UserDataEntity>());

            var exception = Assert.ThrowsAsync<InvalidLoginAttemptException>(async () => await sut.Login(_testEmail, _testPassword));

            Assert.AreEqual($"No user found with email {_testEmail}", exception.Message);
        }

        [Test]
        public void LoginThrowsInvalidLoginAttemptIfInvalidPassword()
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            var expectedUser = GetUserDataEntity();

            expectedUser.Password = GetHashedPassword(expectedUser.Password);

            _mockUserRepository.Setup(x => x.Search(It.IsAny<IEnumerable<SearchParameter>>())).ReturnsAsync(new List<UserDataEntity>()
            {
                expectedUser
            });

            var exception = Assert.ThrowsAsync<InvalidLoginAttemptException>(async () => await sut.Login(_testEmail, "some invalid password"));

            Assert.AreEqual("Invalid credentials given", exception.Message);
        }

        [Test]
        public async Task LoginReturnsUserIfLoginSuccessful()
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            var expectedUser = GetUserDataEntity();
            expectedUser.Password = GetHashedPassword(expectedUser.Password);

            _mockUserRepository.Setup(x => x.Search(It.IsAny<IEnumerable<SearchParameter>>())).ReturnsAsync(new List<UserDataEntity>()
            {
                expectedUser
            });

            var result = await sut.Login(_testEmail, _testPassword);

            Assert.AreEqual(result.Id, expectedUser.Id);
            Assert.AreEqual(result.Email, expectedUser.Email);
            Assert.AreEqual(result.FirstName, expectedUser.FirstName);
            Assert.AreEqual(result.LastName, expectedUser.LastName);
            Assert.AreEqual(result.Password, expectedUser.Password);
        }

        #endregion Login

        #region Register
        
        [Test]
        public async Task RegisterNewUserRunsSuccessfully()
        {
            var user = GetUser();
            var usersHashedPassword = GetHashedPassword(user.Password);
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);

            var addedUserDataEntity = GetUserDataEntity();
            addedUserDataEntity.Password = usersHashedPassword;

            _mockUserRepository.Setup(x => x.Search(It.IsAny<IEnumerable<SearchParameter>>())).ReturnsAsync(new List<UserDataEntity>());
            _mockUserRepository.Setup(x => x.Add(It.IsAny<UserDataEntity>())).ReturnsAsync(addedUserDataEntity);

            user.Id = string.Empty;

            var result = await sut.RegisterNewUser(user);

            _mockUserRepository.Verify(x => x.Add(It.IsAny<UserDataEntity>()));

            Assert.False(string.IsNullOrWhiteSpace(result.Id));
            Assert.AreEqual(user.Email, result.Email);
            Assert.AreEqual(user.FirstName, result.FirstName);
            Assert.AreEqual(user.LastName, result.LastName);
            Assert.AreEqual(usersHashedPassword, result.Password);
        }

        [Test]
        public void RegisterNewUserThrowsIfUserIsNull()
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.RegisterNewUser(null));
            Assert.AreEqual("user", exception.ParamName);
        }

        [Test]
        public void RegisterNewUserThrowsIfUserContainsNullOrEmptyProperties()
        {
            var sut = new UserService(_mockUserRepository.Object, _userMapper, _mockLogger.Object);
            var user = GetUser();
            user.Email = string.Empty;
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.RegisterNewUser(user));
            Assert.AreEqual("Email", exception.ParamName);

            user = GetUser();
            user.FirstName = string.Empty;
            exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.RegisterNewUser(user));
            Assert.AreEqual("FirstName", exception.ParamName);

            user = GetUser();
            user.LastName = string.Empty;
            exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.RegisterNewUser(user));
            Assert.AreEqual("LastName", exception.ParamName);
        }

        #endregion Register

        private static string GetHashedPassword(string password)
        {
            var salt = BCryptHelper.GenerateSalt();
            var hashedPassword = BCryptHelper.HashPassword(password, salt);

            return hashedPassword;
        }

        private static User GetUser() => new User
        {
            Id = _testId,
            Email = _testEmail,
            Password = _testPassword,
            FirstName = "Rob",
            LastName = "Baxter"
        };

        private static UserDataEntity GetUserDataEntity() => new UserDataEntity
        {
            Id = _testId,
            Email = _testEmail,
            Password = _testPassword,
            FirstName = "Rob",
            LastName = "Baxter"
        };
    }
}
