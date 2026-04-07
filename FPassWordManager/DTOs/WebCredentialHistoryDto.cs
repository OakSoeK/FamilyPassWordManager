namespace FPassWordManager.DTOs
{
    public record WebCredentialHistoryDto(Guid WebCredentialHistoryId, string Url, string Username, string? Notes, string ChangeType, DateTime? ChangedAt, string? ChangedByName);
}
