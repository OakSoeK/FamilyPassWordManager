namespace FPassWordManager.DTOs
{
    public record SharedWithMeDto(Guid CredentialId, string CredentialName, string OwnerName, string PermissionLevel, DateTime ExpireAt, bool IsExpired);
}
