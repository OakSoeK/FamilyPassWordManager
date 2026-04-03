using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(SecurityKeyHistoryId))]
    public class SecurityKeyHistory
    {
        public Guid SecurityKeyHistoryId { get; set; }
        public Guid SecurityKeyId { get; set; }
        [MaxLength(50)]
        public string Label { get; set; } = string.Empty;
        [MaxLength(20)]
        public string PinHash { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Notes { get; set; } = string.Empty;
        public Guid ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        [MaxLength(50)]
        public string ChangeType { get; set; } = string.Empty;
        //Navigation
        public SecurityKey SecurityKey { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User? ChangedByUser { get; set; }
    }
}
