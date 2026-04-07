namespace FPassWordManager.DTOs
{
    public record WebCredentialDto(Guid WebCredentialId, Guid CredentialId, string CredentialName, string Url, string Username, DateTime CreatedAt, DateTime? EditedAt, bool CanEdit);
}
