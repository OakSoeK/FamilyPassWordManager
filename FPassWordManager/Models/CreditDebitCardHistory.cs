using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CreditDebitHistoryId))]
    public class CreditDebitCardHistory
    {
        public Guid CreditDebitHistoryId { get; set; }
        public Guid CreditDebitId { get; set; }
        [MaxLength(100)] public string CardHolderName { get; set; } = string.Empty;
        [MaxLength(512)] public string CardNumberHash { get; set; } = string.Empty;
        [MaxLength(2)] public string ExpiryMonth { get; set; } = string.Empty;
        [MaxLength(2)] public string ExpiryYear { get; set; } = string.Empty;
        [MaxLength(512)] public string CvvHash { get; set; } = string.Empty;
        [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
        [MaxLength(100)] public string? BillingAddress { get; set; }
        [MaxLength(200)] public string? Notes { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        [MaxLength(50)] public string ChangeType { get; set; } = string.Empty;

        public CreditDebitCard CreditDebitCard { get; set; } = null!;
        public User? ChangedByUser { get; set; }
    }
}
