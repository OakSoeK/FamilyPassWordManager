namespace FPassWordManager.DTOs
{
    public record SecurityKeyHistoryDto(Guid SecurityKeyHistoryId, string Label, string? Notes, string ChangeType, DateTime ChangedAt, string? ChangedByName);
}
