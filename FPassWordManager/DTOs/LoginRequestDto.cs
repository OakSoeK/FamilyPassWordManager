using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.DTOs
{
    public class LoginRequestDto
    {
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [MaxLength(30)]
        public string Password { get; set; } = string.Empty;

    }
}

