using BaxterStore.Data.Exceptions;
using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using BCrypt;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaxterStore.Service.Implementation.Users
{
    public class UserService : IUserService
    {
        private readonly ICrudRepository<UserDataEntity> _userRepository;
        private readonly IMapper<User, UserDataEntity> _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(ICrudRepository<UserDataEntity> userRepository, IMapper<User, UserDataEntity> mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            _logger.LogTrace("Building search parameter with email {email}", email);
            var searchParams = new List<SearchParameter>
            {
                new SearchParameter { Column = "email", Value = email },
            };

            var expectedUser = (await _userRepository.Search(searchParams))?.FirstOrDefault();

            if (expectedUser is null) throw new InvalidLoginAttemptException($"No user found with email {email}");
            _logger.LogTrace("Found user with email {email} and id {id}", email, expectedUser.Id);

            CheckPassword(password, expectedUser.Password);

            _logger.LogTrace("Successfully validated user with email {email} and id {id}", email, expectedUser.Id);

            return _mapper.MapToResource(expectedUser);
        }

        public async Task<User> RegisterNewUser(User user)
        {
            ValidateUser(user);
            if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentNullException(nameof(user.Password));

            await ValidateExistingUser(user.Email);

            user.Password = HashPassword(user.Password);

            var addedUser = await _userRepository.Add(_mapper.MapToEntity(user));

            _logger.LogTrace("User added for email {email} with new id {id}", user.Email, addedUser.Id);

            return _mapper.MapToResource(addedUser);
        }

        public async Task<User> UpdateUser(User user)
        {
            ValidateUser(user);
            if (string.IsNullOrWhiteSpace(user.Id)) throw new ArgumentNullException(nameof(user.Id));

            await ValidateExistingUser(user.Email);

            _logger.LogTrace("Finding user with id {id}", user.Id);
            var userDataEntity = await _userRepository.FindById(user.Id);

            if (userDataEntity is null) throw new InvalidLoginAttemptException($"No user found with id {user.Id}");

            var mappedUser = _mapper.MapToEntity(user);

            // don't change password if not provided otherwise hash
            mappedUser.Password = string.IsNullOrWhiteSpace(mappedUser.Password) ? userDataEntity.Password : HashPassword(mappedUser.Password);

            _logger.LogTrace("Updating user with id {id}", user.Id);
            var updatedUser = await _userRepository.Update(mappedUser);

            return _mapper.MapToResource(updatedUser);
        }

        private void CheckPassword(string password, string hashedPassword)
        {
            if(!BCryptHelper.CheckPassword(password, hashedPassword)) throw new InvalidLoginAttemptException("Invalid credentials given");
        }

        private string HashPassword(string password)
        {
            var salt = BCryptHelper.GenerateSalt();
            var hashedPassword = BCryptHelper.HashPassword(password, salt);

            return hashedPassword;
        }

        private async Task ValidateExistingUser(string email)
        {
            var searchParams = new List<SearchParameter>
            {
                new SearchParameter { Column = "email", Value = email },
            };

            _logger.LogTrace("Checking if user {email} already exists", email);
            var existingUserList = await _userRepository.Search(searchParams);

            if (existingUserList?.Any() ?? false) throw new DuplicateResourceException($"User with email {email} already exists");

            _logger.LogTrace("Email {email} is available, creating user", email);
        }

        private void ValidateUser(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentNullException(nameof(user.Email));
            if (string.IsNullOrWhiteSpace(user.FirstName)) throw new ArgumentNullException(nameof(user.FirstName));
            if (string.IsNullOrWhiteSpace(user.LastName)) throw new ArgumentNullException(nameof(user.LastName));
        }
    }
}
