using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(SecurityKeyId))]
    public class SecurityKey
    {
        public Guid SecurityKeyId { get; set; }
        public Guid CredentialId { get; set; }
        [MaxLength(100)] public string Label { get; set; } = string.Empty;
        [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
        [MaxLength(200)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? EditedAt { get; set; }
        public Guid? EditorId { get; set; }

        public Credential Credential { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public User? Editor { get; set; }
        public ICollection<SecurityKeyAccess> Accesses { get; set; } = new List<SecurityKeyAccess>();
    }
}
