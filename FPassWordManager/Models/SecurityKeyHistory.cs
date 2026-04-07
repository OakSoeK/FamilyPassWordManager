using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(SecurityKeyHistoryId))]
    public class SecurityKeyHistory
    {
        public Guid SecurityKeyHistoryId { get; set; }
        public Guid SecurityKeyId { get; set; }
        [MaxLength(100)] public string Label { get; set; } = string.Empty;
        [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
        [MaxLength(200)] public string? Notes { get; set; }
        public Guid ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        [MaxLength(50)] public string ChangeType { get; set; } = string.Empty;

        public SecurityKey SecurityKey { get; set; } = null!;
        public User? ChangedByUser { get; set; }
    }
}
