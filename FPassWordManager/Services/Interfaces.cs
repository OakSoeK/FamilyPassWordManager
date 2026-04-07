using FPassWordManager.DTOs;
using FPassWordManager.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace FPassWordManager.Services
{
    public interface ICredentialService
    {
        Task<IEnumerable<CredentialDto>> GetMyCredentialsAsync(Guid userId);
        Task<CredentialDto?> GetByIdAsync(Guid credentialId, Guid userId);
        Task<CredentialDto> CreateAsync(Guid userId, CreateCredentialDto dto);
        Task<CredentialDto?> UpdateAsync(Guid credentialId, Guid userId, UpdateCredentialDto dto);
        Task<bool> DeleteAsync(Guid credentialId, Guid userId);
    }

    public interface IPinService
    {
        Task<bool> VerifyPinAsync(Guid userId, string pin);
    }

    public interface IWebCredentialService
    {
        Task<IEnumerable<WebCredentialDto>> GetByCredentialAsync(Guid credentialId, Guid userId);
        Task<WebCredentialDto?> GetByIdAsync(Guid webCredentialId, Guid userId);
        Task<WebCredentialDto> CreateAsync(Guid userId, CreateWebCredentialDto dto);
        Task<WebCredentialDto?> UpdateAsync(Guid webCredentialId, Guid userId, UpdateWebCredentialDto dto);
        Task<bool> DeleteAsync(Guid webCredentialId, Guid userId);
        Task<IEnumerable<WebCredentialHistoryDto>> GetHistoryAsync(Guid webCredentialId, Guid userId);
    }

    public interface ICreditDebitCardService
    {
        Task<IEnumerable<CreditDebitCardDto>> GetByCredentialAsync(Guid credentialId, Guid userId);
        Task<CreditDebitCardDto?> GetByIdAsync(Guid creditDebitId, Guid userId);
        Task<CreditDebitCardDto> CreateAsync(Guid userId, CreateCreditDebitCardDto dto);
        Task<CreditDebitCardDto?> UpdateAsync(Guid creditDebitId, Guid userId, UpdateCreditDebitCardDto dto);
        Task<bool> DeleteAsync(Guid creditDebitId, Guid userId);
        Task<IEnumerable<CreditDebitCardHistoryDto>> GetHistoryAsync(Guid creditDebitId, Guid userId);
    }

    public interface ISecurityKeyService
    {
        Task<IEnumerable<SecurityKeyDto>> GetByCredentialAsync(Guid credentialId, Guid userId);
        Task<SecurityKeyDto?> GetByIdAsync(Guid securityKeyId, Guid userId);
        Task<SecurityKeyDto> CreateAsync(Guid userId, CreateSecurityKeyDto dto);
        Task<SecurityKeyDto?> UpdateAsync(Guid securityKeyId, Guid userId, UpdateSecurityKeyDto dto);
        Task<bool> DeleteAsync(Guid securityKeyId, Guid userId);
        Task<IEnumerable<SecurityKeyHistoryDto>> GetHistoryAsync(Guid securityKeyId, Guid userId);
    }

    public interface IAccessService
    {
        Task<IEnumerable<AccessDto>> GetAccessListForCredentialAsync(Guid credentialId, Guid ownerId);
        Task<IEnumerable<SharedWithMeDto>> GetSharedWithMeAsync(Guid userId);
        Task<AccessDto> GrantAccessAsync(Guid ownerId, GrantAccessDto dto);
        Task<bool> RevokeAccessAsync(Guid credentialAccessId, Guid ownerId);
        Task<bool> HasAccessAsync(Guid credentialId, Guid userId, string requiredPermission = "View");
        // Returns: "Owner" | "Edit" | "View" | "None"
        Task<string> GetMyPermissionAsync(Guid credentialId, Guid userId);
    }
}
