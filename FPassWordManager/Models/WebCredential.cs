using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(WebCredentialId))]
    public class WebCredential
    {
        public Guid WebCredentialId { get; set; }
        public Guid CredentialId { get; set; }
        [MaxLength(200)] public string Url { get; set; } = string.Empty;
        [MaxLength(100)] public string Username { get; set; } = string.Empty;
        [MaxLength(512)] public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(100)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? EditedAt { get; set; }
        public Guid? EditorId { get; set; }

        public Credential Credential { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public User? Editor { get; set; }
        public ICollection<WebCredentialAccess> Accesses { get; set; } = new List<WebCredentialAccess>();
    }
}
