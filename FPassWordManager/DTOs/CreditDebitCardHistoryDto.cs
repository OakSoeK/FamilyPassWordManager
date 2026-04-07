namespace FPassWordManager.DTOs
{
    public record CreditDebitCardHistoryDto(Guid CreditDebitHistoryId, string CardHolderName, string MaskedCardNumber, string ExpiryMonth, string ExpiryYear, string? Notes, string? BillingAddress, string ChangeType, DateTime ChangedAt, string? ChangedByName);
}
