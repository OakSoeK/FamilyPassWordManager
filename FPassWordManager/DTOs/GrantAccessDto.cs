namespace FPassWordManager.DTOs
{
    public record GrantAccessDto(Guid CredentialId, string SharedToUsername, string PermissionLevel, DateTime ExpireAt);
}
