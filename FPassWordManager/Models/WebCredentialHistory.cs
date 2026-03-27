namespace FPassWordManager.Models
{
    public class WebCredentialHistory
    {
        public int WHid { get; set; }
        public int Wid { get; set; }
        public int Cid { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public string? Notes { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangeType { get; set; }


    }
}
