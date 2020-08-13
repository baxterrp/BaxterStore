using BaxterStore.Api.Contracts;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BaxterStore.Api.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost("/users/login")]
        public async Task<IActionResult> Login([FromBody]LoginContract loginContract)
        {
            return Ok(await _userService.Login(loginContract.Email, loginContract.Password));
        }

        [HttpPost("/users")]
        public async Task<IActionResult> RegisterNewUser([FromBody]User user)
        {
            return Ok(await _userService.RegisterNewUser(user));
        }
    }
}
