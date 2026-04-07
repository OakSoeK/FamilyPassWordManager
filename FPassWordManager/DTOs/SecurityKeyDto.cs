namespace FPassWordManager.DTOs
{
    public record SecurityKeyDto(Guid SecurityKeyId, Guid CredentialId, string CredentialName, string Label, string? Notes, DateTime CreatedAt, DateTime? EditedAt, bool CanEdit);
}
