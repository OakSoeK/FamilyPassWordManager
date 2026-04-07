
using FamilyPasswordManager.Data;
using FPassWordManager.DTOs;
using FPassWordManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FPassWordManager.Services
{
    public class PinService : IPinService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public PinService(AppDbContext db, IPasswordHasher<User> hasher)
        { _db = db; _hasher = hasher; }

        public async Task<bool> VerifyPinAsync(Guid userId, string pin)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            var result = _hasher.VerifyHashedPassword(user, user.PinHash, pin);
            return result != PasswordVerificationResult.Failed;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // AccessService
    // ══════════════════════════════════════════════════════════════════
    public class AccessService : IAccessService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public AccessService(AppDbContext db, UserManager<User> userManager)
        { _db = db; _userManager = userManager; }

        public async Task<bool> HasAccessAsync(Guid credentialId, Guid userId, string requiredPermission = "View")
        {
            bool isOwner = await _db.Credentials.AnyAsync(c => c.CredentialId == credentialId && c.UserId == userId);
            if (isOwner) return true;

            var access = await _db.CredentialAccesses.FirstOrDefaultAsync(a =>
                a.CredentialId == credentialId && a.UserId == userId && a.ExpireAt > DateTime.UtcNow);
            if (access == null) return false;

            return requiredPermission == "View" || access.PermissionLevel == "Edit";
        }

        public async Task<string> GetMyPermissionAsync(Guid credentialId, Guid userId)
        {
            bool isOwner = await _db.Credentials
                .AnyAsync(c => c.CredentialId == credentialId && c.UserId == userId);
            if (isOwner) return "Owner";

            var access = await _db.CredentialAccesses.FirstOrDefaultAsync(a =>
                a.CredentialId == credentialId &&
                a.UserId == userId &&
                a.ExpireAt > DateTime.UtcNow);

            return access?.PermissionLevel ?? "None";
        }

        public async Task<IEnumerable<AccessDto>> GetAccessListForCredentialAsync(Guid credentialId, Guid ownerId)
        {
            // Both owners AND shared users can see the access list (read-only for non-owners)
            bool hasAccess = await HasAccessAsync(credentialId, ownerId);
            if (!hasAccess) return [];

            return await _db.CredentialAccesses
                .Where(a => a.CredentialId == credentialId)
                .Include(a => a.Credential).Include(a => a.User).Include(a => a.SharedByUser)
                .OrderByDescending(a => a.SharedAt)
                .Select(a => new AccessDto(
                    a.CredentialAccessId, a.CredentialId, a.Credential.CredentialName,
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
                .Select(a => new SharedWithMeDto(
                    a.CredentialId, a.Credential.CredentialName,
                    (a.Credential.User.FirstName + " " + a.Credential.User.LastName).Trim(),
                    a.PermissionLevel, a.ExpireAt, a.ExpireAt <= DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<AccessDto> GrantAccessAsync(Guid ownerId, GrantAccessDto dto)
        {
            var credential = await _db.Credentials
                .FirstOrDefaultAsync(c => c.CredentialId == dto.CredentialId && c.UserId == ownerId)
                ?? throw new UnauthorizedAccessException("Credential not found or you are not the owner.");

            var targetUser = await _userManager.FindByNameAsync(dto.SharedToUsername)
                ?? throw new KeyNotFoundException($"User '{dto.SharedToUsername}' not found.");

            if (targetUser.Id == ownerId)
                throw new InvalidOperationException("You cannot share a credential with yourself.");

            var existing = await _db.CredentialAccesses
                .Where(a => a.CredentialId == dto.CredentialId && a.UserId == targetUser.Id)
                .ToListAsync();
            _db.CredentialAccesses.RemoveRange(existing);

            var access = new CredentialAccess
            {
                CredentialAccessId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                UserId = targetUser.Id,
                PermissionLevel = dto.PermissionLevel,
                SharedByUserId = ownerId,
                SharedAt = DateTime.UtcNow,
                ExpireAt = dto.ExpireAt
            };

            _db.CredentialAccesses.Add(access);
            await _db.SaveChangesAsync();

            var sharer = await _userManager.FindByIdAsync(ownerId.ToString());
            return new AccessDto(access.CredentialAccessId, access.CredentialId,
                credential.CredentialName, targetUser.UserName!, sharer!.UserName!,
                access.PermissionLevel, access.SharedAt, access.ExpireAt, false);
        }

        public async Task<bool> RevokeAccessAsync(Guid credentialAccessId, Guid ownerId)
        {
            var access = await _db.CredentialAccesses.Include(a => a.Credential)
                .FirstOrDefaultAsync(a => a.CredentialAccessId == credentialAccessId);
            if (access == null) return false;
            if (access.Credential.UserId != ownerId)
                throw new UnauthorizedAccessException("Only the credential owner can revoke access.");
            _db.CredentialAccesses.Remove(access);
            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // CredentialService
    // ══════════════════════════════════════════════════════════════════
    public class CredentialService : ICredentialService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;

        public CredentialService(AppDbContext db, IAccessService access)
        { _db = db; _access = access; }

        public async Task<IEnumerable<CredentialDto>> GetMyCredentialsAsync(Guid userId) =>
            await _db.Credentials.Where(c => c.UserId == userId).Include(c => c.User)
                .OrderByDescending(c => c.LastEditedAt).Select(c => ToDto(c)).ToListAsync();

        // Allows owners AND shared users to fetch a credential
        public async Task<CredentialDto?> GetByIdAsync(Guid id, Guid userId)
        {
            var hasAccess = await _access.HasAccessAsync(id, userId);
            if (!hasAccess) return null;

            var c = await _db.Credentials.Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CredentialId == id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CredentialDto> CreateAsync(Guid userId, CreateCredentialDto dto)
        {
            var c = new Credential
            {
                CredentialId = Guid.NewGuid(),
                CredentialName = dto.CredentialName,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastEditedAt = DateTime.UtcNow
            };
            _db.Credentials.Add(c);
            await _db.SaveChangesAsync();
            await _db.Entry(c).Reference(x => x.User).LoadAsync();
            return ToDto(c);
        }

        public async Task<CredentialDto?> UpdateAsync(Guid id, Guid userId, UpdateCredentialDto dto)
        {
            // Only owner can rename
            var c = await _db.Credentials.Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CredentialId == id && c.UserId == userId);
            if (c == null) return null;
            c.CredentialName = dto.CredentialName;
            c.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(c);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            // Only owner can delete
            var c = await _db.Credentials
                .FirstOrDefaultAsync(c => c.CredentialId == id && c.UserId == userId);
            if (c == null) return false;
            _db.Credentials.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        private static CredentialDto ToDto(Credential c) => new(
            c.CredentialId, c.CredentialName, c.CreatedAt, c.LastEditedAt,
            c.UserId, $"{c.User?.FirstName} {c.User?.LastName}".Trim());
    }

    // ══════════════════════════════════════════════════════════════════
    // WebCredentialService
    // ══════════════════════════════════════════════════════════════════
    public class WebCredentialService : IWebCredentialService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        private readonly IPasswordHasher<User> _hasher;

        public WebCredentialService(AppDbContext db, IAccessService access, IPasswordHasher<User> hasher)
        { _db = db; _access = access; _hasher = hasher; }

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
            var w = await _db.WebCredentials.Include(w => w.Credential)
                .FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null || !await _access.HasAccessAsync(w.CredentialId, userId)) return null;
            return ToDto(w, await _access.HasAccessAsync(w.CredentialId, userId, "Edit"));
        }

        public async Task<WebCredentialDto> CreateAsync(Guid userId, CreateWebCredentialDto dto)
        {
            // Only owner or Edit users can create
            if (!await _access.HasAccessAsync(dto.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");

            var user = await _db.Users.FindAsync(userId)!;
            var w = new WebCredential
            {
                WebCredentialId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                Url = dto.Url,
                Username = dto.Username,
                PasswordHash = _hasher.HashPassword(user!, dto.Password),
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
            var w = await _db.WebCredentials.Include(w => w.Credential)
                .FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return null;
            if (!await _access.HasAccessAsync(w.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");

            var user = await _db.Users.FindAsync(userId)!;
            SaveWebHistory(w, userId, "Update");
            w.Url = dto.Url; w.Username = dto.Username; w.Notes = dto.Notes;
            w.EditedAt = DateTime.UtcNow; w.EditorId = userId;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                w.PasswordHash = _hasher.HashPassword(user!, dto.Password);
            w.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(w, true);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var w = await _db.WebCredentials.Include(w => w.Credential)
                .FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null) return false;
            // Only owner can delete
            if (w.Credential.UserId != userId)
                throw new UnauthorizedAccessException("Only the owner can delete items.");
            SaveWebHistory(w, userId, "Delete");
            _db.WebCredentials.Remove(w);
            w.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WebCredentialHistoryDto>> GetHistoryAsync(Guid id, Guid userId)
        {
            var w = await _db.WebCredentials.FirstOrDefaultAsync(w => w.WebCredentialId == id);
            if (w == null || !await _access.HasAccessAsync(w.CredentialId, userId)) return [];
            return await _db.WebCredentialsHistorys.Where(h => h.WebCredentialId == id)
                .Include(h => h.ChangedByUser).OrderByDescending(h => h.ChangedAt)
                .Select(h => new WebCredentialHistoryDto(
                    h.WebCredentialHistoryId, h.Url, h.Username, h.Notes, h.ChangeType, h.ChangedAt,
                    h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim() : null))
                .ToListAsync();
        }

        private void SaveWebHistory(WebCredential w, Guid userId, string changeType) =>
            _db.WebCredentialsHistorys.Add(new WebCredentialHistory
            {
                WebCredentialHistoryId = Guid.NewGuid(),
                WebCredentialId = w.WebCredentialId,
                Url = w.Url,
                Username = w.Username,
                PasswordHash = w.PasswordHash,
                Notes = w.Notes,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                ChangeType = changeType
            });

        private static WebCredentialDto ToDto(WebCredential w, bool canEdit) =>
            new(w.WebCredentialId, w.CredentialId, w.Credential?.CredentialName ?? string.Empty,
                w.Url, w.Username, w.CreatedAt, w.EditedAt, canEdit);
    }

    // ══════════════════════════════════════════════════════════════════
    // CreditDebitCardService
    // ══════════════════════════════════════════════════════════════════
    public class CreditDebitCardService : ICreditDebitCardService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        private readonly IPasswordHasher<User> _hasher;

        public CreditDebitCardService(AppDbContext db, IAccessService access, IPasswordHasher<User> hasher)
        { _db = db; _access = access; _hasher = hasher; }

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
            var c = await _db.CreditCards.Include(c => c.Credential)
                .FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (c == null || !await _access.HasAccessAsync(c.CredentialId, userId)) return null;
            return ToDto(c, await _access.HasAccessAsync(c.CredentialId, userId, "Edit"));
        }

        public async Task<CreditDebitCardDto> CreateAsync(Guid userId, CreateCreditDebitCardDto dto)
        {
            if (!await _access.HasAccessAsync(dto.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");

            var user = await _db.Users.FindAsync(userId)!;
            var card = new CreditDebitCard
            {
                CreditDebitId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                CardHolderName = dto.CardHolderName,
                CardNumberHash = _hasher.HashPassword(user!, dto.CardNumber),
                ExpiryMonth = dto.ExpiryMonth,
                ExpiryYear = dto.ExpiryYear,
                CvvHash = _hasher.HashPassword(user!, dto.Cvv),
                PinHash = _hasher.HashPassword(user!, dto.Pin),
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
            var card = await _db.CreditCards.Include(c => c.Credential)
                .FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null) return null;
            if (!await _access.HasAccessAsync(card.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");

            var user = await _db.Users.FindAsync(userId)!;
            SaveCardHistory(card, userId, "Update");
            card.CardHolderName = dto.CardHolderName; card.ExpiryMonth = dto.ExpiryMonth;
            card.ExpiryYear = dto.ExpiryYear; card.BillingAddress = dto.BillingAddress;
            card.Notes = dto.Notes; card.EditedAt = DateTime.UtcNow; card.EditorId = userId;
            if (!string.IsNullOrWhiteSpace(dto.CardNumber)) card.CardNumberHash = _hasher.HashPassword(user!, dto.CardNumber);
            if (!string.IsNullOrWhiteSpace(dto.Cvv)) card.CvvHash = _hasher.HashPassword(user!, dto.Cvv);
            if (!string.IsNullOrWhiteSpace(dto.Pin)) card.PinHash = _hasher.HashPassword(user!, dto.Pin);
            card.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(card, true);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var card = await _db.CreditCards.Include(c => c.Credential)
                .FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null) return false;
            // Only owner can delete
            if (card.Credential.UserId != userId)
                throw new UnauthorizedAccessException("Only the owner can delete items.");
            SaveCardHistory(card, userId, "Delete");
            _db.CreditCards.Remove(card);
            card.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CreditDebitCardHistoryDto>> GetHistoryAsync(Guid id, Guid userId)
        {
            var card = await _db.CreditCards.FirstOrDefaultAsync(c => c.CreditDebitId == id);
            if (card == null || !await _access.HasAccessAsync(card.CredentialId, userId)) return [];
            return await _db.CreditCardsHistorys.Where(h => h.CreditDebitId == id)
                .Include(h => h.ChangedByUser).OrderByDescending(h => h.ChangedAt)
                .Select(h => new CreditDebitCardHistoryDto(
                    h.CreditDebitHistoryId, h.CardHolderName, Mask(h.CardNumberHash),
                    h.ExpiryMonth, h.ExpiryYear, h.Notes, h.BillingAddress, h.ChangeType, h.ChangedAt,
                    h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim() : null))
                .ToListAsync();
        }

        private void SaveCardHistory(CreditDebitCard c, Guid userId, string changeType) =>
            _db.CreditCardsHistorys.Add(new CreditDebitCardHistory
            {
                CreditDebitHistoryId = Guid.NewGuid(),
                CreditDebitId = c.CreditDebitId,
                CardHolderName = c.CardHolderName,
                CardNumberHash = c.CardNumberHash,
                ExpiryMonth = c.ExpiryMonth,
                ExpiryYear = c.ExpiryYear,
                CvvHash = c.CvvHash,
                PinHash = c.PinHash,
                BillingAddress = c.BillingAddress,
                Notes = c.Notes,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                ChangeType = changeType
            });

        private static string Mask(string h) => h.Length >= 4 ? $"**** **** **** {h[^4..]}" : "****";
        private static CreditDebitCardDto ToDto(CreditDebitCard c, bool canEdit) =>
            new(c.CreditDebitId, c.CredentialId, c.Credential?.CredentialName ?? string.Empty,
                c.CardHolderName, Mask(c.CardNumberHash), c.ExpiryMonth, c.ExpiryYear,
                c.BillingAddress, c.Notes, c.CreatedAt, c.EditedAt, canEdit);
    }

    // ══════════════════════════════════════════════════════════════════
    // SecurityKeyService
    // ══════════════════════════════════════════════════════════════════
    public class SecurityKeyService : ISecurityKeyService
    {
        private readonly AppDbContext _db;
        private readonly IAccessService _access;
        private readonly IPasswordHasher<User> _hasher;

        public SecurityKeyService(AppDbContext db, IAccessService access, IPasswordHasher<User> hasher)
        { _db = db; _access = access; _hasher = hasher; }

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
            var s = await _db.SecurityKeys.Include(s => s.Credential)
                .FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (s == null || !await _access.HasAccessAsync(s.CredentialId, userId)) return null;
            return ToDto(s, await _access.HasAccessAsync(s.CredentialId, userId, "Edit"));
        }

        public async Task<SecurityKeyDto> CreateAsync(Guid userId, CreateSecurityKeyDto dto)
        {
            if (!await _access.HasAccessAsync(dto.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");

            var user = await _db.Users.FindAsync(userId)!;
            var key = new SecurityKey
            {
                SecurityKeyId = Guid.NewGuid(),
                CredentialId = dto.CredentialId,
                Label = dto.Label,
                PinHash = _hasher.HashPassword(user!, dto.Pin),
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
            var key = await _db.SecurityKeys.Include(s => s.Credential)
                .FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null) return null;
            if (!await _access.HasAccessAsync(key.CredentialId, userId, "Edit"))
                throw new UnauthorizedAccessException("Edit permission required.");

            var user = await _db.Users.FindAsync(userId)!;
            SaveKeyHistory(key, userId, "Update");
            key.Label = dto.Label; key.Notes = dto.Notes;
            key.EditedAt = DateTime.UtcNow; key.EditorId = userId;
            if (!string.IsNullOrWhiteSpace(dto.Pin))
                key.PinHash = _hasher.HashPassword(user!, dto.Pin);
            key.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(key, true);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var key = await _db.SecurityKeys.Include(s => s.Credential)
                .FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null) return false;
            // Only owner can delete
            if (key.Credential.UserId != userId)
                throw new UnauthorizedAccessException("Only the owner can delete items.");
            SaveKeyHistory(key, userId, "Delete");
            _db.SecurityKeys.Remove(key);
            key.Credential.LastEditedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SecurityKeyHistoryDto>> GetHistoryAsync(Guid id, Guid userId)
        {
            var key = await _db.SecurityKeys.FirstOrDefaultAsync(s => s.SecurityKeyId == id);
            if (key == null || !await _access.HasAccessAsync(key.CredentialId, userId)) return [];
            return await _db.SecurityKeysHistorys.Where(h => h.SecurityKeyId == id)
                .Include(h => h.ChangedByUser).OrderByDescending(h => h.ChangedAt)
                .Select(h => new SecurityKeyHistoryDto(
                    h.SecurityKeyHistoryId, h.Label, h.Notes, h.ChangeType, h.ChangedAt,
                    h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim() : null))
                .ToListAsync();
        }

        private void SaveKeyHistory(SecurityKey k, Guid userId, string changeType) =>
            _db.SecurityKeysHistorys.Add(new SecurityKeyHistory
            {
                SecurityKeyHistoryId = Guid.NewGuid(),
                SecurityKeyId = k.SecurityKeyId,
                Label = k.Label,
                PinHash = k.PinHash,
                Notes = k.Notes,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                ChangeType = changeType
            });

        private static SecurityKeyDto ToDto(SecurityKey s, bool canEdit) =>
            new(s.SecurityKeyId, s.CredentialId, s.Credential?.CredentialName ?? string.Empty,
                s.Label, s.Notes, s.CreatedAt, s.EditedAt, canEdit);
    }
}
