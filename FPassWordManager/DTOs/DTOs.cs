namespace FPassWordManager.DTOs
{
    public record CredentialDto(Guid CredentialId, string CredentialName, DateTime CreatedAt, DateTime LastEditedAt, Guid OwnerId, string OwnerName);
    public record CreateCredentialDto(string CredentialName);
    public record UpdateCredentialDto(string CredentialName);
    public record VerifyPinDto(string Pin);

    public record WebCredentialDto(Guid WebCredentialId, Guid CredentialId, string CredentialName, string Url, string Username, string? Notes, DateTime CreatedAt, DateTime? EditedAt, bool CanEdit);
    public record CreateWebCredentialDto(Guid CredentialId, string Url, string Username, string Password, string? Notes);
    public record UpdateWebCredentialDto(string Url, string Username, string? Password, string? Notes);
    public record WebCredentialHistoryDto(Guid WebCredentialHistoryId, string Url, string Username, string? Notes, string ChangeType, DateTime? ChangedAt, string? ChangedByName);
    public record RevealWebDto(string Password);

    public record CreditDebitCardDto(Guid CreditDebitId, Guid CredentialId, string CredentialName, string CardHolderName, string MaskedCardNumber, string ExpiryMonth, string ExpiryYear, string? BillingAddress, string? Notes, DateTime CreatedAt, DateTime? EditedAt, bool CanEdit);
    public record CreateCreditDebitCardDto(Guid CredentialId, string CardHolderName, string CardNumber, string ExpiryMonth, string ExpiryYear, string Cvv, string Pin, string? BillingAddress, string? Notes);
    public record UpdateCreditDebitCardDto(string CardHolderName, string? CardNumber, string ExpiryMonth, string ExpiryYear, string? Cvv, string? Pin, string? BillingAddress, string? Notes);
    public record CreditDebitCardHistoryDto(Guid CreditDebitHistoryId, string CardHolderName, string MaskedCardNumber, string ExpiryMonth, string ExpiryYear, string? Notes, string? BillingAddress, string ChangeType, DateTime ChangedAt, string? ChangedByName);
    public record RevealCardDto(string CardNumber, string Cvv, string Pin);

    public record SecurityKeyDto(Guid SecurityKeyId, Guid CredentialId, string CredentialName, string Label, string? Notes, DateTime CreatedAt, DateTime? EditedAt, bool CanEdit);
    public record CreateSecurityKeyDto(Guid CredentialId, string Label, string Pin, string? Notes);
    public record UpdateSecurityKeyDto(string Label, string? Pin, string? Notes);
    public record SecurityKeyHistoryDto(Guid SecurityKeyHistoryId, string Label, string? Notes, string ChangeType, DateTime ChangedAt, string? ChangedByName);
    public record RevealKeyDto(string Pin);

    public record GrantAccessDto(Guid CredentialId, string SharedToUsername, string PermissionLevel, DateTime ExpireAt);
    public record AccessDto(Guid CredentialAccessId, Guid CredentialId, string CredentialName, string SharedToUsername, string SharedByUsername, string PermissionLevel, DateTime SharedAt, DateTime ExpireAt, bool IsExpired);
    public record SharedWithMeDto(Guid CredentialId, string CredentialName, string OwnerName, string PermissionLevel, DateTime ExpireAt, bool IsExpired);

    public record ApiResponse<T>(bool Success, string? Message, T? Data);

    public record GrantItemAccessDto(string SharedToUsername, string PermissionLevel, DateTime ExpireAt);
    public record ItemAccessDto(Guid AccessId, string SharedToUsername, string SharedByUsername, string PermissionLevel, DateTime SharedAt, DateTime ExpireAt, bool IsExpired);
    public record SharedItemDto(string ItemType, Guid ItemId, string ItemTitle, string ItemSubtitle, Guid CredentialId, string CredentialName, string OwnerName, string PermissionLevel, DateTime ExpireAt, bool IsExpired);
}
