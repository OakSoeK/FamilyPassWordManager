using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{

        //User
        public class User : IdentityUser<Guid>
        {
            [MaxLength(30)] public string FirstName { get; set; } = string.Empty;
            [MaxLength(30)] public string? LastName { get; set; }
            [MaxLength(30)] public string? PhNumber { get; set; }
            [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? LastActive { get; set; }

            public ICollection<Credential> Credentials { get; set; } = new List<Credential>();
        }
        //Credential

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
        //CredentialAccess

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

        //WebCredential
        [PrimaryKey(nameof(WebCredentialId))]
        public class WebCredential
        {
            public Guid WebCredentialId { get; set; }
            public Guid CredentialId { get; set; }
            [MaxLength(200)] public string Url { get; set; } = string.Empty;
            [MaxLength(100)] public string Username { get; set; } = string.Empty;
            [MaxLength(512)] public string PasswordHash { get; set; } = string.Empty;
            [MaxLength(100)] public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatorId { get; set; }
            public DateTime? EditedAt { get; set; }
            public Guid? EditorId { get; set; }

            public Credential Credential { get; set; } = null!;
            public User Creator { get; set; } = null!;
            public User? Editor { get; set; }
            public ICollection<WebCredentialAccess> Accesses { get; set; } = new List<WebCredentialAccess>();
        }

        //WebCredentialAccess
        [PrimaryKey(nameof(WebCredentialAccessId))]
        public class WebCredentialAccess
        {
            public Guid WebCredentialAccessId { get; set; }
            public Guid WebCredentialId { get; set; }
            public Guid UserId { get; set; }
            [MaxLength(50)] public string PermissionLevel { get; set; } = string.Empty;
            public DateTime ExpireAt { get; set; }
            public DateTime SharedAt { get; set; }
            public Guid SharedByUserId { get; set; }

            public WebCredential WebCredential { get; set; } = null!;
            public User SharedByUser { get; set; } = null!;
            public User User { get; set; } = null!;
        }
        
        //WebCredentialHistory
        [PrimaryKey(nameof(WebCredentialHistoryId))]
        public class WebCredentialHistory
        {
            public Guid WebCredentialHistoryId { get; set; }
            public Guid WebCredentialId { get; set; }
            [MaxLength(200)] public string Url { get; set; } = string.Empty;
            [MaxLength(100)] public string Username { get; set; } = string.Empty;
            [MaxLength(512)] public string PasswordHash { get; set; } = string.Empty;
            [MaxLength(100)] public string? Notes { get; set; }
            public Guid? ChangedByUserId { get; set; }
            public DateTime? ChangedAt { get; set; }
            [MaxLength(50)] public string ChangeType { get; set; } = string.Empty;

            public WebCredential WebCredential { get; set; } = null!;
            public User? ChangedByUser { get; set; }
        }

        //CreditDebit
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

        //CreditDebitAccess
        [PrimaryKey(nameof(CreditDebitAccessId))]
        public class CreditDebitCardAccess
        {
            public Guid CreditDebitAccessId { get; set; }
            public Guid CreditDebitId { get; set; }
            public Guid UserId { get; set; }
            [MaxLength(50)] public string PermissionLevel { get; set; } = string.Empty;
            public DateTime ExpireAt { get; set; }
            public DateTime SharedAt { get; set; }
            public Guid SharedByUserId { get; set; }

            public CreditDebitCard CreditDebitCard { get; set; } = null!;
            public User SharedByUser { get; set; } = null!;
            public User User { get; set; } = null!;
        }

        //CreditDebitHistory
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

        //SecurityKey
        [PrimaryKey(nameof(SecurityKeyId))]
        public class SecurityKey
        {
            public Guid SecurityKeyId { get; set; }
            public Guid CredentialId { get; set; }
            [MaxLength(100)] public string Label { get; set; } = string.Empty;
            [MaxLength(512)] public string PinHash { get; set; } = string.Empty;
            [MaxLength(200)] public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatorId { get; set; }
            public DateTime? EditedAt { get; set; }
            public Guid? EditorId { get; set; }

            public Credential Credential { get; set; } = null!;
            public User Creator { get; set; } = null!;
            public User? Editor { get; set; }
            public ICollection<SecurityKeyAccess> Accesses { get; set; } = new List<SecurityKeyAccess>();
        }

        //SecurityKeyAccess
        [PrimaryKey(nameof(SecurityKeyAccessId))]
        public class SecurityKeyAccess
        {
            public Guid SecurityKeyAccessId { get; set; }
            public Guid SecurityKeyId { get; set; }
            public Guid UserId { get; set; }
            [MaxLength(50)] public string PermissionLevel { get; set; } = string.Empty;
            public DateTime ExpireAt { get; set; }
            public DateTime SharedAt { get; set; }
            public Guid SharedByUserId { get; set; }

            public SecurityKey SecurityKey { get; set; } = null!;
            public User SharedByUser { get; set; } = null!;
            public User User { get; set; } = null!;
        }

        //SecurityKeyHistory
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


