namespace FPassWordManager.Models
{
    public class CreditDebitCard
    {
        public int CDid { get; set; }
        public int Cid { get; set; }
        public string CardHolderName { get; set; }
        public byte[] CardNumberHash { get; set; }
        public byte ExpiryMonth { get; set; }
        public byte ExpiryYear { get; set; }
        public byte[] CvvHash { get; set; }
        public string? BillingAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime EditedAt { get; set; }
        public int EditedBy { get; set; }

    }
}
