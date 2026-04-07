namespace FPassWordManager.DTOs
{
    public record CreateWebCredentialDto(Guid CredentialId, string Url, string Username, string Password, string? Notes);
}
