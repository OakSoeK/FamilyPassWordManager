using FPasswordManager.Data;
using FPassWordManager.DTOs;
using FPassWordManager.Models;
using FPassWordManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FPasswordManager.Services
{
    //sensitive info encryption
    public static class AesEncryption
    {
        public static string Encrypt(string plainText, string base64Key)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(base64Key);
            aes.GenerateIV();
            using var enc = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = enc.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText, string base64Key)
        {
            var parts = cipherText.Split(':', 2);
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(base64Key);
            aes.IV = Convert.FromBase64String(parts[0]);
            using var dec = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(parts[1]);
            return Encoding.UTF8.GetString(dec.TransformFinalBlock(bytes, 0, bytes.Length));
        }
    }

    //pin check
    public class PinService : IPinService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        public PinService(AppDbContext db, IPasswordHasher<User> hasher) { _db = db; _hasher = hasher; }

        public async Task<bool> VerifyPinAsync(Guid userId, string pin)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            return _hasher.VerifyHashedPassword(user, user.PinHash, pin) != PasswordVerificationResult.Failed;
        }
    }

    //FolderAccessService

    public class AccessService : IAccessService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        public AccessService(AppDbContext db, UserManager<User> userManager) { _db = db; _userManager = userManager; }

        public async Task<bool> HasAccessAsync(Guid credentialId, Guid userId, string requiredPermission = "View")
        {
            if (await _db.Credentials.AnyAsync(c => c.CredentialId == credentialId && c.UserId == userId)) return true;
            var access = await _db.CredentialAccesses.FirstOrDefaultAsync(a =>
                a.CredentialId == credentialId && a.UserId == userId && a.ExpireAt > DateTime.UtcNow);
            if (access == null) return false;
            return requiredPermission == "View" || access.PermissionLevel == "Edit";
        }

        public async Task<string> GetMyPermissionAsync(Guid credentialId, Guid userId)
        {
            if (await _db.Credentials.AnyAsync(c => c.CredentialId == credentialId && c.UserId == userId)) return "Owner";
            var access = await _db.CredentialAccesses.FirstOrDefaultAsync(a =>
                a.CredentialId == credentialId && a.UserId == userId && a.ExpireAt > DateTime.UtcNow);
            return access?.PermissionLevel ?? "None";
        }

        public async Task<IEnumerable<AccessDto>> GetAccessListForCredentialAsync(Guid credentialId, Guid ownerId)
        {
            if (!await HasAccessAsync(credentialId, ownerId)) return [];
            return await _db.CredentialAccesses
                .Where(a => a.CredentialId == credentialId)
                .Include(a => a.Credential).Include(a => a.User).Include(a => a.SharedByUser)
                .OrderByDescending(a => a.SharedAt)
                .Select(a => new AccessDto(a.CredentialAccessId, a.CredentialId, a.Credential.CredentialName,
                    a.User.UserName!, a.SharedByUser.UserName!, a.PermissionLevel,
                    a.SharedAt, a.ExpireAt, a.ExpireAt <= DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<IEnumerable<SharedWithMeDto>> GetSharedWithMeAsync(Guid userId)
        {
            return await _db.CredentialAccesses
                .Where(a => a.UserId == userId && a.ExpireAt > DateTime.UtcNow)
                .Include(a => a.Credential).ThenInclude(c => c.User)
                .OrderByDescending(a => a.SharedAt)
                .Select(a => new SharedWithMeDto(a.CredentialId, a.Credential.CredentialName,
                    (a.Credential.User.FirstName + " " + a.Credential.User.LastName).Trim(),
                    a.PermissionLevel, a.ExpireAt, a.ExpireAt <= DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<AccessDto> GrantAccessAsync(Guid ownerId, GrantAccessDto dto)
        {
            var credential = await _db.Credentials.FirstOrDefaultAsync(c => c.CredentialId == dto.CredentialId && c.UserId == ownerId)
                ?? throw new UnauthorizedAccessException("Not the owner.");
            var targetUser = await _userManager.FindByNameAsync(dto.SharedToUsername)
                ?? throw new KeyNotFoundException($"User '{dto.SharedToUsername}' not found.");
            if (targetUser.Id == ownerId) throw new InvalidOperationException("Cannot share with yourself.");

            var existing = await _db.CredentialAccesses.Where(a => a.CredentialId == dto.CredentialId && a.UserId == targetUser.Id).ToListAsync();
            _db.CredentialAccesses.RemoveRange(existing);

            var access = new CredentialAccess { CredentialAccessId = Guid.NewGuid(), CredentialId = dto.CredentialId, UserId = targetUser.Id, PermissionLevel = dto.PermissionLevel, SharedByUserId = ownerId, SharedAt = DateTime.UtcNow, ExpireAt = dto.ExpireAt };
            _db.CredentialAccesses.Add(access);
            await _db.SaveChangesAsync();

            var sharer = await _userManager.FindByIdAsync(ownerId.ToString());
            return new AccessDto(access.CredentialAccessId, access.CredentialId, credential.CredentialName,
                targetUser.UserName!, sharer!.UserName!, access.PermissionLevel, access.SharedAt, access.ExpireAt, false);
        }

        public async Task<bool> RevokeAccessAsync(Guid credentialAccessId, Guid ownerId)
        {
            var access = await _db.CredentialAccesses.Include(a => a.Credential).FirstOrDefaultAsync(a => a.CredentialAccessId == credentialAccessId);
            if (access == null) return false;
            if (access.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can revoke access.");
            _db.CredentialAccesses.Remove(access);
            await _db.SaveChangesAsync();
            return true;
        }
    }

    //CredentialService

    public class CredentialService : ICredentialService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        public CredentialService(AppDbContext db, IAccessService access) { _db = db; _access = access; }

        public async Task<IEnumerable<CredentialDto>> GetMyCredentialsAsync(Guid userId) =>
            await _db.Credentials.Where(c => c.UserId == userId).Include(c => c.User)
                .OrderByDescending(c => c.LastEditedAt).Select(c => ToDto(c)).ToListAsync();

        public async Task<CredentialDto?> GetByIdAsync(Guid id, Guid userId)
        {
            if (!await _access.HasAccessAsync(id, userId)) return null;
            var c = await _db.Credentials.Include(c => c.User).FirstOrDefaultAsync(c => c.CredentialId == id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CredentialDto> CreateAsync(Guid userId, CreateCredentialDto dto)
        {
            var c = new Credential { CredentialId = Guid.NewGuid(), CredentialName = dto.CredentialName, UserId = userId, CreatedAt = DateTime.UtcNow, LastEditedAt = DateTime.UtcNow };
            _db.Credentials.Add(c);
            await _db.SaveChangesAsync();
            await _db.Entry(c).Reference(x => x.User).LoadAsync();
            return ToDto(c);
        }

        public async Task<CredentialDto?> UpdateAsync(Guid id, Guid userId, UpdateCredentialDto dto)
        {
            var c = await _db.Credentials.Include(c => c.User).FirstOrDefaultAsync(c => c.CredentialId == id && c.UserId == userId);
            if (c == null) return null;
            c.CredentialName = dto.CredentialName; c.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ToDto(c);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var c = await _db.Credentials.FirstOrDefaultAsync(c => c.CredentialId == id && c.UserId == userId);
            if (c == null) return false;
            _db.Credentials.Remove(c); await _db.SaveChangesAsync(); return true;
        }

        private static CredentialDto ToDto(Credential c) =>
            new(c.CredentialId, c.CredentialName, c.CreatedAt, c.LastEditedAt, c.UserId, $"{c.User?.FirstName} {c.User?.LastName}".Trim());
    }

    // WebCredentialService 
    public class WebCredentialService : IWebCredentialService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        private readonly IItemAccessService _itemAccess;
        private readonly IConfiguration _config;
        public WebCredentialService(AppDbContext db, IAccessService access, IItemAccessService itemAccess, IConfiguration config)
        { _db = db; _access = access; _itemAccess = itemAccess; _config = config; }

        private string Key => _config["AppSettings:EncryptionKey"]!;

        public async Task<IEnumerable<WebCredentialDto>> GetByCredentialAsync(Guid credentialId, Guid userId)
        {
            if (!await _access.HasAccessAsync(credentialId, userId)) return [];
            bool canEdit = await _access.HasAccessAsync(credentialId, userId, "Edit");
            return await _db.WebCredentials.Where(w => w.CredentialId == credentialId)
                .Include(w => w.Credential).OrderByDescending(w => w.CreatedAt)
                .Select(w => ToDto(w, canEdit)).ToListAsync();
        }

        public async Task<WebCredentialDto?> GetByIdAsync(Guid id, Guid userId)
        {
            var w = await _db.WebCredentials.Include(w => w.Credential).FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return null;
            bool allowed = await _access.HasAccessAsync(w.CredentialId, userId)
                        || await _itemAccess.HasWebAccessAsync(id, userId);
            if (!allowed) return null;
            bool canEdit = await _access.HasAccessAsync(w.CredentialId, userId, "Edit")
                        || await _itemAccess.HasWebAccessAsync(id, userId, "Edit");
            return ToDto(w, canEdit);
        }

        public async Task<WebCredentialDto> CreateAsync(Guid userId, CreateWebCredentialDto dto)
        {
            if (!await _access.HasAccessAsync(dto.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");
            var w = new WebCredential
            {
                WebCredentialId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                Url = dto.Url,
                Username = dto.Username,
                PasswordHash = AesEncryption.Encrypt(dto.Password, Key),
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatorId = userId
            };
            _db.WebCredentials.Add(w);
            var cred = await _db.Credentials.FindAsync(dto.CredentialId);
            if (cred != null) cred.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _db.Entry(w).Reference(x => x.Credential).LoadAsync();
            return ToDto(w, true);
        }

        public async Task<WebCredentialDto?> UpdateAsync(Guid id, Guid userId, UpdateWebCredentialDto dto)
        {
            var w = await _db.WebCredentials.Include(w => w.Credential).FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return null;
            bool canEdit = await _access.HasAccessAsync(w.CredentialId, userId, "Edit")
                        || await _itemAccess.HasWebAccessAsync(id, userId, "Edit");
            if (!canEdit) throw new UnauthorizedAccessException("Edit permission required.");
            SaveHistory(w, userId, "Update");
            w.Url = dto.Url; w.Username = dto.Username; w.Notes = dto.Notes; w.EditedAt = DateTime.UtcNow; w.EditorId = userId;
            if (!string.IsNullOrWhiteSpace(dto.Password)) w.PasswordHash = AesEncryption.Encrypt(dto.Password, Key);
            w.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ToDto(w, true);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var w = await _db.WebCredentials.Include(w => w.Credential).FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return false;
            if (w.Credential.UserId != userId) throw new UnauthorizedAccessException("Only the owner can delete.");
            SaveHistory(w, userId, "Delete");
            _db.WebCredentials.Remove(w); w.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return true;
        }

        public async Task<IEnumerable<WebCredentialHistoryDto>> GetHistoryAsync(Guid id, Guid userId)
        {
            var w = await _db.WebCredentials.FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return [];
            bool allowed = await _access.HasAccessAsync(w.CredentialId, userId)
                        || await _itemAccess.HasWebAccessAsync(id, userId);
            if (!allowed) return [];
            return await _db.WebCredentialsHistorys.Where(h => h.WebCredentialId == id)
                .Include(h => h.ChangedByUser).OrderByDescending(h => h.ChangedAt)
                .Select(h => new WebCredentialHistoryDto(h.WebCredentialHistoryId, h.Url, h.Username, h.Notes, h.ChangeType, h.ChangedAt,
                    h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim() : null))
                .ToListAsync();
        }

        public async Task<RevealWebDto?> RevealAsync(Guid id, Guid userId)
        {
            var w = await _db.WebCredentials.Include(w => w.Credential).FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return null;
            bool allowed = await _access.HasAccessAsync(w.CredentialId, userId)
                        || await _itemAccess.HasWebAccessAsync(id, userId);
            if (!allowed) return null;
            return new RevealWebDto(AesEncryption.Decrypt(w.PasswordHash, Key));
        }

        private void SaveHistory(WebCredential w, Guid userId, string t) =>
            _db.WebCredentialsHistorys.Add(new WebCredentialHistory { WebCredentialHistoryId = Guid.NewGuid(), WebCredentialId = w.WebCredentialId, Url = w.Url, Username = w.Username, PasswordHash = w.PasswordHash, Notes = w.Notes, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, ChangeType = t });

        private static WebCredentialDto ToDto(WebCredential w, bool canEdit) =>
            new(w.WebCredentialId, w.CredentialId, w.Credential?.CredentialName ?? string.Empty, w.Url, w.Username, w.Notes, w.CreatedAt, w.EditedAt, canEdit);
    }

    // CreditDebitCardService 

    public class CreditDebitCardService : ICreditDebitCardService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        private readonly IItemAccessService _itemAccess;
        private readonly IConfiguration _config;
        public CreditDebitCardService(AppDbContext db, IAccessService access, IItemAccessService itemAccess, IConfiguration config)
        { _db = db; _access = access; _itemAccess = itemAccess; _config = config; }

        private string Key => _config["AppSettings:EncryptionKey"]!;

        public async Task<IEnumerable<CreditDebitCardDto>> GetByCredentialAsync(Guid credentialId, Guid userId)
        {
            if (!await _access.HasAccessAsync(credentialId, userId)) return [];
            bool canEdit = await _access.HasAccessAsync(credentialId, userId, "Edit");
            return await _db.CreditCards.Where(c => c.CredentialId == credentialId)
                .Include(c => c.Credential).OrderByDescending(c => c.CreatedAt)
                .Select(c => ToDto(c, canEdit)).ToListAsync();
        }

        public async Task<CreditDebitCardDto?> GetByIdAsync(Guid id, Guid userId)
        {
            var c = await _db.CreditCards.Include(c => c.Credential).FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (c == null) return null;
            bool allowed = await _access.HasAccessAsync(c.CredentialId, userId)
                        || await _itemAccess.HasCardAccessAsync(id, userId);
            if (!allowed) return null;
            bool canEdit = await _access.HasAccessAsync(c.CredentialId, userId, "Edit")
                        || await _itemAccess.HasCardAccessAsync(id, userId, "Edit");
            return ToDto(c, canEdit);
        }

        public async Task<CreditDebitCardDto> CreateAsync(Guid userId, CreateCreditDebitCardDto dto)
        {
            if (!await _access.HasAccessAsync(dto.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");
            var card = new CreditDebitCard
            {
                CreditDebitId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                CardHolderName = dto.CardHolderName,
                CardNumberHash = AesEncryption.Encrypt(dto.CardNumber, Key),
                ExpiryMonth = dto.ExpiryMonth,
                ExpiryYear = dto.ExpiryYear,
                CvvHash = AesEncryption.Encrypt(dto.Cvv, Key),
                PinHash = AesEncryption.Encrypt(dto.Pin, Key),
                BillingAddress = dto.BillingAddress,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatorId = userId
            };
            _db.CreditCards.Add(card);
            var cred = await _db.Credentials.FindAsync(dto.CredentialId);
            if (cred != null) cred.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _db.Entry(card).Reference(c => c.Credential).LoadAsync();
            return ToDto(card, true);
        }

        public async Task<CreditDebitCardDto?> UpdateAsync(Guid id, Guid userId, UpdateCreditDebitCardDto dto)
        {
            var card = await _db.CreditCards.Include(c => c.Credential).FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null) return null;
            bool canEdit = await _access.HasAccessAsync(card.CredentialId, userId, "Edit")
                        || await _itemAccess.HasCardAccessAsync(id, userId, "Edit");
            if (!canEdit) throw new UnauthorizedAccessException("Edit permission required.");
            SaveHistory(card, userId, "Update");
            card.CardHolderName = dto.CardHolderName; card.ExpiryMonth = dto.ExpiryMonth; card.ExpiryYear = dto.ExpiryYear;
            card.BillingAddress = dto.BillingAddress; card.Notes = dto.Notes; card.EditedAt = DateTime.UtcNow; card.EditorId = userId;
            if (!string.IsNullOrWhiteSpace(dto.CardNumber)) card.CardNumberHash = AesEncryption.Encrypt(dto.CardNumber, Key);
            if (!string.IsNullOrWhiteSpace(dto.Cvv)) card.CvvHash = AesEncryption.Encrypt(dto.Cvv, Key);
            if (!string.IsNullOrWhiteSpace(dto.Pin)) card.PinHash = AesEncryption.Encrypt(dto.Pin, Key);
            card.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ToDto(card, true);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var card = await _db.CreditCards.Include(c => c.Credential).FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null) return false;
            if (card.Credential.UserId != userId) throw new UnauthorizedAccessException("Only the owner can delete.");
            SaveHistory(card, userId, "Delete");
            _db.CreditCards.Remove(card); card.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return true;
        }

        public async Task<IEnumerable<CreditDebitCardHistoryDto>> GetHistoryAsync(Guid id, Guid userId)
        {
            var card = await _db.CreditCards.FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null) return [];
            bool allowed = await _access.HasAccessAsync(card.CredentialId, userId)
                        || await _itemAccess.HasCardAccessAsync(id, userId);
            if (!allowed) return [];
            return await _db.CreditCardsHistorys.Where(h => h.CreditDebitId == id)
                .Include(h => h.ChangedByUser).OrderByDescending(h => h.ChangedAt)
                .Select(h => new CreditDebitCardHistoryDto(h.CreditDebitHistoryId, h.CardHolderName, MaskStored(h.CardNumberHash),
                    h.ExpiryMonth, h.ExpiryYear, h.Notes, h.BillingAddress, h.ChangeType, h.ChangedAt,
                    h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim() : null))
                .ToListAsync();
        }

        public async Task<RevealCardDto?> RevealAsync(Guid id, Guid userId)
        {
            var card = await _db.CreditCards.Include(c => c.Credential).FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null) return null;
            bool allowed = await _access.HasAccessAsync(card.CredentialId, userId)
                        || await _itemAccess.HasCardAccessAsync(id, userId);
            if (!allowed) return null;
            return new RevealCardDto(
                AesEncryption.Decrypt(card.CardNumberHash, Key),
                AesEncryption.Decrypt(card.CvvHash, Key),
                AesEncryption.Decrypt(card.PinHash, Key));
        }

        private void SaveHistory(CreditDebitCard c, Guid userId, string t) =>
            _db.CreditCardsHistorys.Add(new CreditDebitCardHistory { CreditDebitHistoryId = Guid.NewGuid(), CreditDebitId = c.CreditDebitId, CardHolderName = c.CardHolderName, CardNumberHash = c.CardNumberHash, ExpiryMonth = c.ExpiryMonth, ExpiryYear = c.ExpiryYear, CvvHash = c.CvvHash, PinHash = c.PinHash, BillingAddress = c.BillingAddress, Notes = c.Notes, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, ChangeType = t });

        private static string MaskStored(string enc) => "**** **** **** ????";
        private static CreditDebitCardDto ToDto(CreditDebitCard c, bool canEdit) =>
            new(c.CreditDebitId, c.CredentialId, c.Credential?.CredentialName ?? string.Empty,
                c.CardHolderName, "**** **** **** ????", c.ExpiryMonth, c.ExpiryYear,
                c.BillingAddress, c.Notes, c.CreatedAt, c.EditedAt, canEdit);
    }

    //SecurityKeyService

    public class SecurityKeyService : ISecurityKeyService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        private readonly IItemAccessService _itemAccess;
        private readonly IConfiguration _config;
        public SecurityKeyService(AppDbContext db, IAccessService access, IItemAccessService itemAccess, IConfiguration config)
        { _db = db; _access = access; _itemAccess = itemAccess; _config = config; }

        private string Key => _config["AppSettings:EncryptionKey"]!;

        public async Task<IEnumerable<SecurityKeyDto>> GetByCredentialAsync(Guid credentialId, Guid userId)
        {
            if (!await _access.HasAccessAsync(credentialId, userId)) return [];
            bool canEdit = await _access.HasAccessAsync(credentialId, userId, "Edit");
            return await _db.SecurityKeys.Where(s => s.CredentialId == credentialId)
                .Include(s => s.Credential).OrderByDescending(s => s.CreatedAt)
                .Select(s => ToDto(s, canEdit)).ToListAsync();
        }

        public async Task<SecurityKeyDto?> GetByIdAsync(Guid id, Guid userId)
        {
            var s = await _db.SecurityKeys.Include(s => s.Credential).FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (s == null) return null;
            bool allowed = await _access.HasAccessAsync(s.CredentialId, userId)
                        || await _itemAccess.HasKeyAccessAsync(id, userId);
            if (!allowed) return null;
            bool canEdit = await _access.HasAccessAsync(s.CredentialId, userId, "Edit")
                        || await _itemAccess.HasKeyAccessAsync(id, userId, "Edit");
            return ToDto(s, canEdit);
        }

        public async Task<SecurityKeyDto> CreateAsync(Guid userId, CreateSecurityKeyDto dto)
        {
            if (!await _access.HasAccessAsync(dto.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");
            var key = new SecurityKey
            {
                SecurityKeyId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                Label = dto.Label,
                PinHash = AesEncryption.Encrypt(dto.Pin, Key),
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatorId = userId
            };
            _db.SecurityKeys.Add(key);
            var cred = await _db.Credentials.FindAsync(dto.CredentialId);
            if (cred != null) cred.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _db.Entry(key).Reference(k => k.Credential).LoadAsync();
            return ToDto(key, true);
        }

        public async Task<SecurityKeyDto?> UpdateAsync(Guid id, Guid userId, UpdateSecurityKeyDto dto)
        {
            var key = await _db.SecurityKeys.Include(s => s.Credential).FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null) return null;
            bool canEdit = await _access.HasAccessAsync(key.CredentialId, userId, "Edit")
                        || await _itemAccess.HasKeyAccessAsync(id, userId, "Edit");
            if (!canEdit) throw new UnauthorizedAccessException("Edit permission required.");
            SaveHistory(key, userId, "Update");
            key.Label = dto.Label; key.Notes = dto.Notes; key.EditedAt = DateTime.UtcNow; key.EditorId = userId;
            if (!string.IsNullOrWhiteSpace(dto.Pin)) key.PinHash = AesEncryption.Encrypt(dto.Pin, Key);
            key.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ToDto(key, true);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var key = await _db.SecurityKeys.Include(s => s.Credential).FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null) return false;
            if (key.Credential.UserId != userId) throw new UnauthorizedAccessException("Only the owner can delete.");
            SaveHistory(key, userId, "Delete");
            _db.SecurityKeys.Remove(key); key.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return true;
        }

        public async Task<IEnumerable<SecurityKeyHistoryDto>> GetHistoryAsync(Guid id, Guid userId)
        {
            var key = await _db.SecurityKeys.FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null) return [];
            bool allowed = await _access.HasAccessAsync(key.CredentialId, userId)
                        || await _itemAccess.HasKeyAccessAsync(id, userId);
            if (!allowed) return [];
            return await _db.SecurityKeysHistorys.Where(h => h.SecurityKeyId == id)
                .Include(h => h.ChangedByUser).OrderByDescending(h => h.ChangedAt)
                .Select(h => new SecurityKeyHistoryDto(h.SecurityKeyHistoryId, h.Label, h.Notes, h.ChangeType, h.ChangedAt,
                    h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim() : null))
                .ToListAsync();
        }

        public async Task<RevealKeyDto?> RevealAsync(Guid id, Guid userId)
        {
            var key = await _db.SecurityKeys.Include(s => s.Credential).FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null) return null;
            bool allowed = await _access.HasAccessAsync(key.CredentialId, userId)
                        || await _itemAccess.HasKeyAccessAsync(id, userId);
            if (!allowed) return null;
            return new RevealKeyDto(AesEncryption.Decrypt(key.PinHash, Key));
        }

        private void SaveHistory(SecurityKey k, Guid userId, string t) =>
            _db.SecurityKeysHistorys.Add(new SecurityKeyHistory { SecurityKeyHistoryId = Guid.NewGuid(), SecurityKeyId = k.SecurityKeyId, Label = k.Label, PinHash = k.PinHash, Notes = k.Notes, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, ChangeType = t });

        private static SecurityKeyDto ToDto(SecurityKey s, bool canEdit) =>
            new(s.SecurityKeyId, s.CredentialId, s.Credential?.CredentialName ?? string.Empty, s.Label, s.Notes, s.CreatedAt, s.EditedAt, canEdit);
    }

    //ItemAccessService 

    public class ItemAccessService : IItemAccessService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        public ItemAccessService(AppDbContext db, UserManager<User> userManager)
        { _db = db; _userManager = userManager; }

        private static ItemAccessDto ToDto(WebCredentialAccess a) =>
            new(a.WebCredentialAccessId, a.User.UserName!, a.SharedByUser.UserName!,
                a.PermissionLevel, a.SharedAt, a.ExpireAt, a.ExpireAt <= DateTime.UtcNow);

        private static ItemAccessDto ToDto(CreditDebitCardAccess a) =>
            new(a.CreditDebitAccessId, a.User.UserName!, a.SharedByUser.UserName!,
                a.PermissionLevel, a.SharedAt, a.ExpireAt, a.ExpireAt <= DateTime.UtcNow);

        private static ItemAccessDto ToDto(SecurityKeyAccess a) =>
            new(a.SecurityKeyAccessId, a.User.UserName!, a.SharedByUser.UserName!,
                a.PermissionLevel, a.SharedAt, a.ExpireAt, a.ExpireAt <= DateTime.UtcNow);

        // WC

        public async Task<bool> HasWebAccessAsync(Guid webCredentialId, Guid userId, string required = "View")
        {
            var w = await _db.WebCredentials.Include(x => x.Credential).FirstOrDefaultAsync(x => x.WebCredentialId == webCredentialId);
            if (w == null) return false;
            if (w.Credential.UserId == userId) return true;
            var a = await _db.WebCredentialAccesses.FirstOrDefaultAsync(x =>
                x.WebCredentialId == webCredentialId && x.UserId == userId && x.ExpireAt > DateTime.UtcNow);
            if (a == null) return false;
            return required == "View" || a.PermissionLevel == "Edit";
        }

        public async Task<IEnumerable<ItemAccessDto>> GetWebAccessListAsync(Guid webCredentialId, Guid requesterId)
        {
            if (!await HasWebAccessAsync(webCredentialId, requesterId)) return [];
            return await _db.WebCredentialAccesses
                .Where(a => a.WebCredentialId == webCredentialId)
                .Include(a => a.User).Include(a => a.SharedByUser)
                .OrderByDescending(a => a.SharedAt)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<ItemAccessDto> GrantWebAccessAsync(Guid webCredentialId, Guid ownerId, GrantItemAccessDto dto)
        {
            var w = await _db.WebCredentials.Include(x => x.Credential)
                .FirstOrDefaultAsync(x => x.WebCredentialId == webCredentialId)
                ?? throw new KeyNotFoundException("Item not found.");
            if (w.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can share.");
            var target = await _userManager.FindByNameAsync(dto.SharedToUsername)
                ?? throw new KeyNotFoundException($"User '{dto.SharedToUsername}' not found.");
            if (target.Id == ownerId) throw new InvalidOperationException("Cannot share with yourself.");
            var existing = await _db.WebCredentialAccesses
                .Where(a => a.WebCredentialId == webCredentialId && a.UserId == target.Id).ToListAsync();
            _db.WebCredentialAccesses.RemoveRange(existing);
            var access = new WebCredentialAccess
            {
                WebCredentialAccessId = Guid.NewGuid(),
                WebCredentialId = webCredentialId,
                UserId = target.Id,
                PermissionLevel = dto.PermissionLevel,
                SharedByUserId = ownerId,
                SharedAt = DateTime.UtcNow,
                ExpireAt = dto.ExpireAt
            };
            _db.WebCredentialAccesses.Add(access);
            await _db.SaveChangesAsync();
            await _db.Entry(access).Reference(a => a.User).LoadAsync();
            await _db.Entry(access).Reference(a => a.SharedByUser).LoadAsync();
            return ToDto(access);
        }

        public async Task<bool> RevokeWebAccessAsync(Guid accessId, Guid ownerId)
        {
            var a = await _db.WebCredentialAccesses.Include(x => x.WebCredential).ThenInclude(w => w.Credential)
                .FirstOrDefaultAsync(x => x.WebCredentialAccessId == accessId);
            if (a == null) return false;
            if (a.WebCredential.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can revoke.");
            _db.WebCredentialAccesses.Remove(a);
            await _db.SaveChangesAsync();
            return true;
        }

        // CD

        public async Task<bool> HasCardAccessAsync(Guid creditDebitId, Guid userId, string required = "View")
        {
            var c = await _db.CreditCards.Include(x => x.Credential).FirstOrDefaultAsync(x => x.CreditDebitId == creditDebitId);
            if (c == null) return false;
            if (c.Credential.UserId == userId) return true;
            var a = await _db.CreditDebitCardAccesses.FirstOrDefaultAsync(x =>
                x.CreditDebitId == creditDebitId && x.UserId == userId && x.ExpireAt > DateTime.UtcNow);
            if (a == null) return false;
            return required == "View" || a.PermissionLevel == "Edit";
        }

        public async Task<IEnumerable<ItemAccessDto>> GetCardAccessListAsync(Guid creditDebitId, Guid requesterId)
        {
            if (!await HasCardAccessAsync(creditDebitId, requesterId)) return [];
            return await _db.CreditDebitCardAccesses
                .Where(a => a.CreditDebitId == creditDebitId)
                .Include(a => a.User).Include(a => a.SharedByUser)
                .OrderByDescending(a => a.SharedAt)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<ItemAccessDto> GrantCardAccessAsync(Guid creditDebitId, Guid ownerId, GrantItemAccessDto dto)
        {
            var c = await _db.CreditCards.Include(x => x.Credential)
                .FirstOrDefaultAsync(x => x.CreditDebitId == creditDebitId)
                ?? throw new KeyNotFoundException("Item not found.");
            if (c.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can share.");
            var target = await _userManager.FindByNameAsync(dto.SharedToUsername)
                ?? throw new KeyNotFoundException($"User '{dto.SharedToUsername}' not found.");
            if (target.Id == ownerId) throw new InvalidOperationException("Cannot share with yourself.");
            var existing = await _db.CreditDebitCardAccesses
                .Where(a => a.CreditDebitId == creditDebitId && a.UserId == target.Id).ToListAsync();
            _db.CreditDebitCardAccesses.RemoveRange(existing);
            var access = new CreditDebitCardAccess
            {
                CreditDebitAccessId = Guid.NewGuid(),
                CreditDebitId = creditDebitId,
                UserId = target.Id,
                PermissionLevel = dto.PermissionLevel,
                SharedByUserId = ownerId,
                SharedAt = DateTime.UtcNow,
                ExpireAt = dto.ExpireAt
            };
            _db.CreditDebitCardAccesses.Add(access);
            await _db.SaveChangesAsync();
            await _db.Entry(access).Reference(a => a.User).LoadAsync();
            await _db.Entry(access).Reference(a => a.SharedByUser).LoadAsync();
            return ToDto(access);
        }

        public async Task<bool> RevokeCardAccessAsync(Guid accessId, Guid ownerId)
        {
            var a = await _db.CreditDebitCardAccesses.Include(x => x.CreditDebitCard).ThenInclude(c => c.Credential)
                .FirstOrDefaultAsync(x => x.CreditDebitAccessId == accessId);
            if (a == null) return false;
            if (a.CreditDebitCard.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can revoke.");
            _db.CreditDebitCardAccesses.Remove(a);
            await _db.SaveChangesAsync();
            return true;
        }

        // SK

        public async Task<bool> HasKeyAccessAsync(Guid securityKeyId, Guid userId, string required = "View")
        {
            var k = await _db.SecurityKeys.Include(x => x.Credential).FirstOrDefaultAsync(x => x.SecurityKeyId == securityKeyId);
            if (k == null) return false;
            if (k.Credential.UserId == userId) return true;
            var a = await _db.SecurityKeyAccesses.FirstOrDefaultAsync(x =>
                x.SecurityKeyId == securityKeyId && x.UserId == userId && x.ExpireAt > DateTime.UtcNow);
            if (a == null) return false;
            return required == "View" || a.PermissionLevel == "Edit";
        }

        public async Task<IEnumerable<ItemAccessDto>> GetKeyAccessListAsync(Guid securityKeyId, Guid requesterId)
        {
            if (!await HasKeyAccessAsync(securityKeyId, requesterId)) return [];
            return await _db.SecurityKeyAccesses
                .Where(a => a.SecurityKeyId == securityKeyId)
                .Include(a => a.User).Include(a => a.SharedByUser)
                .OrderByDescending(a => a.SharedAt)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<ItemAccessDto> GrantKeyAccessAsync(Guid securityKeyId, Guid ownerId, GrantItemAccessDto dto)
        {
            var k = await _db.SecurityKeys.Include(x => x.Credential)
                .FirstOrDefaultAsync(x => x.SecurityKeyId == securityKeyId)
                ?? throw new KeyNotFoundException("Item not found.");
            if (k.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can share.");
            var target = await _userManager.FindByNameAsync(dto.SharedToUsername)
                ?? throw new KeyNotFoundException($"User '{dto.SharedToUsername}' not found.");
            if (target.Id == ownerId) throw new InvalidOperationException("Cannot share with yourself.");
            var existing = await _db.SecurityKeyAccesses
                .Where(a => a.SecurityKeyId == securityKeyId && a.UserId == target.Id).ToListAsync();
            _db.SecurityKeyAccesses.RemoveRange(existing);
            var access = new SecurityKeyAccess
            {
                SecurityKeyAccessId = Guid.NewGuid(),
                SecurityKeyId = securityKeyId,
                UserId = target.Id,
                PermissionLevel = dto.PermissionLevel,
                SharedByUserId = ownerId,
                SharedAt = DateTime.UtcNow,
                ExpireAt = dto.ExpireAt
            };
            _db.SecurityKeyAccesses.Add(access);
            await _db.SaveChangesAsync();
            await _db.Entry(access).Reference(a => a.User).LoadAsync();
            await _db.Entry(access).Reference(a => a.SharedByUser).LoadAsync();
            return ToDto(access);
        }

        public async Task<bool> RevokeKeyAccessAsync(Guid accessId, Guid ownerId)
        {
            var a = await _db.SecurityKeyAccesses.Include(x => x.SecurityKey).ThenInclude(k => k.Credential)
                .FirstOrDefaultAsync(x => x.SecurityKeyAccessId == accessId);
            if (a == null) return false;
            if (a.SecurityKey.Credential.UserId != ownerId) throw new UnauthorizedAccessException("Only the owner can revoke.");
            _db.SecurityKeyAccesses.Remove(a);
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<SharedItemDto>> GetAllSharedWithMeAsync(Guid userId)
        {
            var result = new List<SharedItemDto>();
            var allWebAccesses = await _db.WebCredentialAccesses
                .Where(a => a.UserId == userId && a.ExpireAt > DateTime.UtcNow)
                .Include(a => a.WebCredential).ThenInclude(w => w.Credential).ThenInclude(c => c.User)
                .OrderByDescending(a => a.SharedAt)
                .ToListAsync();
            var webAccesses = allWebAccesses
                .GroupBy(a => a.WebCredentialId)
                .Select(g => g.First())
                .ToList();
            foreach (var a in webAccesses)
            {
                var w = a.WebCredential;
                result.Add(new SharedItemDto("web", w.WebCredentialId, w.Username, w.Url,
                    w.CredentialId, w.Credential.CredentialName,
                    $"{w.Credential.User.FirstName} {w.Credential.User.LastName}".Trim(),
                    a.PermissionLevel, a.ExpireAt, false));
            }

            var allCardAccesses = await _db.CreditDebitCardAccesses
                .Where(a => a.UserId == userId && a.ExpireAt > DateTime.UtcNow)
                .Include(a => a.CreditDebitCard).ThenInclude(c => c.Credential).ThenInclude(cr => cr.User)
                .OrderByDescending(a => a.SharedAt)
                .ToListAsync();
            var cardAccesses = allCardAccesses
                .GroupBy(a => a.CreditDebitId)
                .Select(g => g.First())
                .ToList();
            foreach (var a in cardAccesses)
            {
                var c = a.CreditDebitCard;
                result.Add(new SharedItemDto("card", c.CreditDebitId, c.CardHolderName,
                    "**** **** **** ???? · " + c.ExpiryMonth + "/" + c.ExpiryYear,
                    c.CredentialId, c.Credential.CredentialName,
                    $"{c.Credential.User.FirstName} {c.Credential.User.LastName}".Trim(),
                    a.PermissionLevel, a.ExpireAt, false));
            }

            var allKeyAccesses = await _db.SecurityKeyAccesses
                .Where(a => a.UserId == userId && a.ExpireAt > DateTime.UtcNow)
                .Include(a => a.SecurityKey).ThenInclude(k => k.Credential).ThenInclude(cr => cr.User)
                .OrderByDescending(a => a.SharedAt)
                .ToListAsync();
            var keyAccesses = allKeyAccesses
                .GroupBy(a => a.SecurityKeyId)
                .Select(g => g.First())
                .ToList();
            foreach (var a in keyAccesses)
            {
                var k = a.SecurityKey;
                result.Add(new SharedItemDto("key", k.SecurityKeyId, k.Label,
                    k.Notes ?? "Security key",
                    k.CredentialId, k.Credential.CredentialName,
                    $"{k.Credential.User.FirstName} {k.Credential.User.LastName}".Trim(),
                    a.PermissionLevel, a.ExpireAt, false));
            }

            return result.GroupBy(x => x.ItemId).Select(g => g.First()).OrderBy(x => x.OwnerName).ThenBy(x => x.ItemType);
        }
    }
}