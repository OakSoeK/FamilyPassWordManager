namespace FPassWordManager.DTOs
{
    public class UserDto
    {
        public int Uid { get; set; }
        public string Username { get; set; } 
        public string FirstName { get; set; } 
        public string? LastName { get; set; }
        public string? PhNumber { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? LastActive { get; set; }
    }
}