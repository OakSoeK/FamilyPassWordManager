namespace FPassWordManager.DTOs
{
    public record CredentialDto(Guid CredentialId, string CredentialName, DateTime CreatedAt, DateTime LastEditedAt, Guid OwnerId, string OwnerName);
}
