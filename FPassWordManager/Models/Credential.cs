namespace FPassWordManager.Models
{
    public class Credential
    {
        public int Cid { get; set; }
        public int Uid { get; set; } 
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEditedAt { get; set; }
        public int LastEditedBy { get; set; }


    }
}
