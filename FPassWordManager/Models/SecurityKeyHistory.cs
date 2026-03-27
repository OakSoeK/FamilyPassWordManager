namespace FPassWordManager.Models
{
    public class SecurityKeyHistory
    {
        public int SKHid { get; set; }
        public int SKid { get; set; }
        public int Cid { get; set; }
        public string Label { get; set; }
        public byte[] PasswordHash { get; set; }
        public string? Notes { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangeType { get; set; }

    }
}
