using FPassWordManager.DTOs;
using FPassWordManager.Models;
using FPassWordManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FPassWordManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService AuthService) : ControllerBase
    {
        public static User user = new User();
        [HttpPost("register")]
        public async Task<ActionResult<User>>Register(UserRegisterRequestDto request)
        {
           var user = await AuthService.RegisterAsync(request);
            if (user is null) 
                return BadRequest("Username already exists!");
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>>Login(LoginRequestDto request)
        {
              var token = await AuthService.LoginAsync(request);
            if (token is null)
                return BadRequest("Invalid username or password!");
            return Ok(token);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthOnlyEndPoint()
        {
            return Ok("Authenticated!");
        }
        
    }
}
