namespace LalaHealthCare.DataAccess.Models;

public class AuthDtos
{
    public class LoginRequest
    {
        public string Username { get; set; } 
        public string Password { get; set; } 
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public User? User { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class BiometricLoginRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public string BiometricData { get; set; } = string.Empty;
    }
}
