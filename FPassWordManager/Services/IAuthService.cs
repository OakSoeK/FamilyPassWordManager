using FPassWordManager.DTOs;
using FPassWordManager.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace FPassWordManager.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserRegisterRequestDto request);
        Task<string?> LoginAsync(LoginRequestDto request);
    }
}
