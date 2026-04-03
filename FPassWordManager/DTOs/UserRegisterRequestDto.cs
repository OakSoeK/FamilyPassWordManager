using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.DTOs
{
    public class UserRegisterRequestDto
    {
        [MaxLength(30)]
        public string FirstName { get; set; } = string.Empty;
        [MaxLength(30)]
        public string? LastName { get; set; } = string.Empty;
        [MaxLength(30)]
        public string? PhNumber { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [MaxLength(30)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(30)]
        public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(5)]
        public string PinHash { get; set; } = string.Empty;
    }
}
