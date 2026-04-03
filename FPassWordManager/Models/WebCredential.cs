using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(WebCredentialId))]
    public class WebCredential
    {
        public Guid WebCredentialId { get; set; }
        public Guid CredentialId { get; set; }
        [MaxLength(50)]
        public string Url { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [MaxLength(50)]
        public string PasswordHash { get; set; } = string.Empty;
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
        public ICollection<WebCredentialAccess> Accesses { get; set; } =new List<WebCredentialAccess>();   

    }
}
