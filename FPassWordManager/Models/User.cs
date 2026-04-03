using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(UserId))]
    public class User
    {
        public Guid UserId { get; set; }
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActive { get; set; }
        //Navigation

        public ICollection<Credential> Credentials { get; set; } =new List<Credential>();
    }
}
