using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CredentialAccessId))]
    public class CredentialAccess
    {
        public Guid CredentialAccessId { get; set; }
        public Guid CredentialId { get; set; }
        public Guid UserId { get; set; }
        [MaxLength(50)] public string PermissionLevel { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public DateTime SharedAt { get; set; }
        public Guid SharedByUserId { get; set; }

        public Credential Credential { get; set; } = null!;
        public User SharedByUser { get; set; } = null!;
        public User User { get; set; } = null!;
    }

}
