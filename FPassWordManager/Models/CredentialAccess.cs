namespace FPassWordManager.Models
{
    public class CredentialAccess
    {
        public int CAid { get; set; }
        public int Cid { get; set; }
        public int Uid { get; set; }
        public string PermissionLevel { get; set; }
        public string AccessType { get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime SharedAt { get; set; }
        public int SharedBy { get; set; }

    }
}
