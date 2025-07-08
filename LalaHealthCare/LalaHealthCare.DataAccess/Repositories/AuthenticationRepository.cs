using LalaHealthCare.DataAccess.Models;
using LalaHealthCare.DataAccess.ServicesHttp;
using static LalaHealthCare.DataAccess.Models.AuthDtos;

namespace LalaHealthCare.DataAccess.Repositories;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly ApiClientService _apiClient;
    private readonly IConnectivity _connectivity;

    public AuthenticationRepository(
        ApiClientService apiClient,
        IConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public async Task<LoginResponse> AuthenticateAsync(string username, string password)
    {

        // Verificar conectividad
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = "No internet connection available"
            };
        }

        // Crear el request según lo que veo en Postman
        var authRequest = new AuthenticationRequest
        {
            username = username,
            password = password,
            fromMobileApp = true,
            platform = DeviceInfo.Platform.ToString(),
            appVersion = AppInfo.VersionString,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Llamar al endpoint
        var response = await _apiClient.PostAsync<AuthenticationRequest, AuthenticationResponse>(
            "/api/Account/SignIn",
            authRequest, 
            AuthenticationType.Basic);

        if (response == null)
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = "Failed to connect to server"
            };
        }

        if (response.success)
        {
            // Crear el objeto User desde la respuesta
            var user = new User
            {
                Id = response.user.id,
                ClinicianId = response.user.clinicianId,
                Username = response.user.username,
                FullName = response.user.fullName,
                Email = response.user.email,
                Role = response.user.roles?.FirstOrDefault() ?? "Nurse",
                ProfilePictureUrl = $"https://i.pravatar.cc/150?u={response.user.username}",
                PreferredLanguage = "en",
                LastLogin = DateTime.Now
            };


            return new LoginResponse
            {
                Success = true,
                Token = response.token,
                User = user
            };
        }
        else
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = response.error ?? response.message ?? "Authentication failed"
            };
        }
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            return null;
        }

        // Este endpoint podría no existir en tu API actual
        // Si no existe, podrías usar el token JWT para extraer la información del usuario
        var endpoint = $"/api/users/{userId}";
        var userResponse = await _apiClient.GetAsync<UserResponse>(endpoint);

        if (userResponse != null)
        {
            return new User
            {
                Id = userResponse.id,
                Username = userResponse.username,
                FullName = userResponse.fullName,
                Email = userResponse.email,
                Role = userResponse.roles?.FirstOrDefault() ?? "Nurse",
                ProfilePictureUrl = userResponse.profilePictureUrl ?? $"https://i.pravatar.cc/150?u={userResponse.username}",
                PreferredLanguage = userResponse.preferredLanguage ?? "en"
            };
        }

        return null;
    }

    // DTOs internos que coinciden con la respuesta de la API
    private class AuthenticationRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool fromMobileApp { get; set; }
        public string platform { get; set; }
        public string appVersion { get; set; }
        public long timestamp { get; set; }
    }

    private class AuthenticationResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
        public string tokenType { get; set; }
        public string quickLoginToken { get; set; }
        public string quickLoginUrl { get; set; }
        public UserInfo user { get; set; }
        public string redirectUrl { get; set; }
        public string message { get; set; }
        public string error { get; set; }
        public int expiresIn { get; set; }
    }

    private class UserInfo
    {
        public int id { get; set; }
        public string clinicianId { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public List<string> roles { get; set; }
    }

    private class UserResponse
    {
        public int id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public List<string> roles { get; set; }
        public string profilePictureUrl { get; set; }
        public string preferredLanguage { get; set; }
    }
}
