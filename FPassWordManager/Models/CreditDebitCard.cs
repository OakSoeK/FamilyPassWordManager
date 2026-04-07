using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CreditDebitId))]
    public class CreditDebitCard
    {
        public Guid CreditDebitId { get; set; }
        public Guid CredentialId { get; set; }
        [MaxLength(100)] public string CardHolderName { get; set; } = string.Empty;
        [MaxLength(512)] public string CardNumberHash { get; set; } = string.Empty;
        [MaxLength(2)] public string ExpiryMonth { get; set; } = string.Empty;
        [MaxLength(2)] public string ExpiryYear { get; set; } = string.Empty;
        [MaxLength(512)] public string CvvHash { get; set; } = string.Empty;
        [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
        [MaxLength(100)] public string? BillingAddress { get; set; }
        [MaxLength(200)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime? EditedAt { get; set; }
        public Guid? EditorId { get; set; }

        public Credential Credential { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public User? Editor { get; set; }
        public ICollection<CreditDebitCardAccess> Accesses { get; set; } = new List<CreditDebitCardAccess>();
    }
}
