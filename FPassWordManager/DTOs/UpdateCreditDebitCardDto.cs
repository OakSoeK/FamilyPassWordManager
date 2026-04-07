namespace FPassWordManager.DTOs
{
    public record UpdateCreditDebitCardDto(string CardHolderName, string? CardNumber, string ExpiryMonth, string ExpiryYear, string? Cvv, string? Pin, string? BillingAddress, string? Notes);
}
