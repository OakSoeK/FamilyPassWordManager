using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CredentialId))]
    public class Credential
    {
        public Guid CredentialId { get; set; }
        [MaxLength(50)] public string CredentialName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEditedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<CredentialAccess> CredentialAccesses { get; set; } = new List<CredentialAccess>();
    }
}
