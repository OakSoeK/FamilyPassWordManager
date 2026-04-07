namespace FPassWordManager.DTOs
{
    public record CreditDebitCardDto(Guid CreditDebitId, Guid CredentialId, string CredentialName, string CardHolderName, string MaskedCardNumber, string ExpiryMonth, string ExpiryYear, string? BillingAddress, string? Notes, DateTime CreatedAt, DateTime? EditedAt, bool CanEdit);
}
