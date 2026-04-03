using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(SecurityKeyId))]
    public class SecurityKey
    {
        public Guid SecurityKeyId { get; set; }
        public Guid CredentialId { get; set; }
        [MaxLength(20)]
        public string PinHash { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Label { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? EditedAt { get; set; }
        public Guid? EditorId { get; set; }
        //Navigation
        public Credential Credential { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User Creator { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User? Editor { get; set; }
        public ICollection<SecurityKeyAccess> Accesses { get; set; }=new List<SecurityKeyAccess>();  

    }
}
