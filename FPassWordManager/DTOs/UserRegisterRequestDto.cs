namespace FPassWordManager.DTOs
{
    public class UserRegisterRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? PhNumber { get; set; }
    }
}
