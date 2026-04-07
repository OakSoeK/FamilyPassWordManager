namespace FPassWordManager.DTOs
{
    public record CreateSecurityKeyDto(Guid CredentialId, string Label, string Pin, string? Notes);
}
