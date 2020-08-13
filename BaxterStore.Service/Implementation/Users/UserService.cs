using BaxterStore.Data.Exceptions;
using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using BCrypt;
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

        public UserService(ICrudRepository<UserDataEntity> userRepository, IMapper<User, UserDataEntity> mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            
            var searchParams = new List<SearchParameter>
            {
                new SearchParameter { Column = "email", Value = email },
            };

            var expectedUser = (await _userRepository.Search(searchParams))?.FirstOrDefault();

            if (expectedUser is null) throw new InvalidLoginAttemptException($"No user found with email {email}");

            CheckPassword(password, expectedUser.Password);
            
            return _mapper.MapToResource(expectedUser);
        }

        public async Task<User> RegisterNewUser(User user)
        {
            ValidateUser(user);
            if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentNullException(nameof(user.Password));

            await ValidateExistingUser(user.Email);

            user.Password = HashPassword(user.Password);

            var addedUser = await _userRepository.Add(_mapper.MapToEntity(user));

            return _mapper.MapToResource(addedUser);
        }

        public async Task<User> UpdateUser(User user)
        {
            ValidateUser(user);
            if (string.IsNullOrWhiteSpace(user.Id)) throw new ArgumentNullException(nameof(user.Id));

            await ValidateExistingUser(user.Email);
            
            var userDataEntity = await _userRepository.FindById(user.Id);

            if (userDataEntity is null) throw new InvalidLoginAttemptException($"No user found with id {user.Id}");

            var mappedUser = _mapper.MapToEntity(user);

            // don't change password if not provided otherwise hash
            mappedUser.Password = string.IsNullOrWhiteSpace(mappedUser.Password) ? userDataEntity.Password : HashPassword(mappedUser.Password);

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

            var existingUserList = await _userRepository.Search(searchParams);

            if (existingUserList?.Any() ?? false) throw new DuplicateResourceException($"User with email {email} already exists");
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
