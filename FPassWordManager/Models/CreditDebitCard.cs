using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CreditDebitId))]
    public class CreditDebitCard
    {
        public Guid CreditDebitId { get; set; }
        public Guid CredentialId { get; set; }
        [MaxLength(50)]
        public string CardHolderName { get; set; } = string.Empty;
        [MaxLength(30)]
        public string CardNumberHash { get; set; } = string.Empty;
        [MaxLength(2)]
        public string ExpiryMonth { get; set; } = string.Empty;
        [MaxLength(2)]
        public string ExpiryYear { get; set; } = string.Empty;
        [MaxLength(3)]
        public string CvvHash { get; set; } = string.Empty;
        [MaxLength(10)]
        public string PinHash { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? BillingAddress { get; set; } = string.Empty;
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
        public ICollection<CreditDebitCardAccess> Accesses { get; set; } =new List<CreditDebitCardAccess>();
    }
}
