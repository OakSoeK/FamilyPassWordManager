namespace FPassWordManager.DTOs
{
    public record AccessDto(Guid CredentialAccessId, Guid CredentialId, string CredentialName, string SharedToUsername, string SharedByUsername, string PermissionLevel, DateTime SharedAt, DateTime ExpireAt, bool IsExpired);
}
