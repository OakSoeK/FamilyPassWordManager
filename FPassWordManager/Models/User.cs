namespace FPassWordManager.Models
{
    public class User
    {
        public int Uid { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhNumber { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PinHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActive { get; set; }

    }
}
