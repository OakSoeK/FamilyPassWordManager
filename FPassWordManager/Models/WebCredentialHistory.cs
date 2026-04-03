using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(WebCredentialHistoryId))]
    public class WebCredentialHistory
    {
        public Guid WebCredentialHistoryId { get; set; }
        public Guid WebCredentialId { get; set; }
        [MaxLength(50)]
        public string Url { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [MaxLength(50)]
        public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Notes { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public DateTime? ChangedAt { get; set; }
        [MaxLength(50)]
        public string ChangeType { get; set; } = string.Empty;
        //Navigation
        public WebCredential WebCredential { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User? ChangedByUser { get; set; }


    }
}
