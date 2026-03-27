namespace FPassWordManager.Models
{
    public class CreditDebitCardHistory
    {
        public int CDHid { get; set; }
        public int CDid { get; set; }
        public int Cid { get; set; }
        public string CardHolderName { get; set; }
        public byte[] CardNumberHash { get; set; }
        public byte ExpiryMonth { get; set; }
        public byte ExpiryYear { get; set; }
        public byte[] CvvHash { get; set; }
        public string? Notes { get; set; }
        public string? BillingAddress { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangeType { get; set; }

    }
}
