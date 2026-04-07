using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    public class User : IdentityUser<Guid>
    {
        [MaxLength(30)] public string FirstName { get; set; } = string.Empty;
        [MaxLength(30)] public string? LastName { get; set; }
        [MaxLength(30)] public string? PhNumber { get; set; }
        [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActive { get; set; }

        public ICollection<Credential> Credentials { get; set; } = new List<Credential>();
    }
}
