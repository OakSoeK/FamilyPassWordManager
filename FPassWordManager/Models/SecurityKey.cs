namespace FPassWordManager.Models
{
    public class SecurityKey
    {
        public int SKid { get; set; }
        public int Cid { get; set; }
        public string Label { get; set; }
        public byte[] PasswordHash { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime EditedAt { get; set; }
        public int EditedBy { get; set; }

    }
}
