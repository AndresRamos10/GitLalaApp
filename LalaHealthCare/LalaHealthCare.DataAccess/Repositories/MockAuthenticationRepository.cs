using LalaHealthCare.DataAccess.Models;
using System.Text.Json;
using static LalaHealthCare.DataAccess.Models.AuthDtos;

namespace LalaHealthCare.DataAccess.Repositories
{
    public class MockAuthenticationRepository //: IAuthenticationRepository
    {
        private readonly List<User> _mockUsers = new()
    {
        new User
        {
            Id = 1,
            Username = "nursenotes@nursenotes.com",
            FullName = "nursenotes",
            Email = "nursenotes@nursenotes.com",
            Role = "Nurse",
            ProfilePictureUrl = "https://i.pravatar.cc/150?img=1",
            PreferredLanguage = "en"
        },
        new User
        {
            Id = 2,
            Username = "demo",
            FullName = "Demo User",
            Email = "demo@lalahealthcare.com",
            Role = "Nurse",
            ProfilePictureUrl = "https://i.pravatar.cc/150?img=2",
            PreferredLanguage = "es"
        }
    };

        private readonly Dictionary<string, string> _mockPasswords = new()
    {
        { "nursenotes@nursenotes.com", "123456" },
        { "demo", "demo123" }
    };

        public async Task<LoginResponse> AuthenticateAsync(string username, string password)
        {
            await Task.Delay(500); // Simular latencia de red

            var user = _mockUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Usuario no encontrado"
                };
            }

            if (!_mockPasswords.ContainsKey(username.ToLower()) || _mockPasswords[username.ToLower()] != password)
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Contraseña incorrecta"
                };
            }

            user.LastLogin = DateTime.Now;

            return new LoginResponse
            {
                Success = true,
                Token = GenerateMockToken(user),
                User = user
            };
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            await Task.Delay(100);
            return _mockUsers.FirstOrDefault(u => u.Id.ToString() == userId);
        }

        private string GenerateMockToken(User user)
        {
            var tokenData = new
            {
                userId = user.Id,
                username = user.Username,
                exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()
            };

            return Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(tokenData));
        }
    }
}
