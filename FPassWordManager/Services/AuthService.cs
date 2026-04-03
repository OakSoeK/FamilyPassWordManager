using FPassWordManager.Data;
using FPassWordManager.DTOs;
using FPassWordManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FPassWordManager.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<string?> LoginAsync(LoginRequestDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null) {
                return null;

            }
            var result = new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }
            return createToken(user);
        }

        public async Task<User?> RegisterAsync(UserRegisterRequestDto request)
        {
            if (await context.Users.AnyAsync(u =>u.Username == request.Username))
            {
                return null;
            }
            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);
            var hashedPin = new PasswordHasher<User>()
                .HashPassword(user, request.PinHash);
            user.Username = request.Username;
            user.PhNumber = request.PhNumber;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PinHash = hashedPin;
            user.PasswordHash = hashedPassword;
            user.CreatedAt = DateTime.UtcNow;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
        private string createToken(User user)
        {
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("Appsettings:Issuer"),
                audience: configuration.GetValue<string>("Appsettings:Audience"),
                claims: claim,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
