using LalaHealthCare.DataAccess.Models;
using LalaHealthCare.DataAccess.Repositories;
using static LalaHealthCare.DataAccess.Models.AuthDtos;

namespace LalaHealthCare.Business.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationRepository _authRepository;
    private User? _currentUser;
    private string? _currentToken;

    public AuthenticationService(IAuthenticationRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentToken);

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _authRepository.AuthenticateAsync(request.Username, request.Password);

            if (response.Success)
            {
                _currentUser = response.User;
                _currentToken = response.Token;
            }

            return response;
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}"
            };
        }
    }

    public async Task<LoginResponse> BiometricLoginAsync(BiometricLoginRequest request)
    {
        // Por ahora, simularemos que el login biométrico siempre funciona
        // En una implementación real, esto validaría los datos biométricos
        await Task.Delay(1000);

        // Simular login con el usuario demo
        return await LoginAsync(new LoginRequest
        {
            Username = "nursenotes@nursenotes.com",
            Password = "123456"
        });
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        _currentToken = null;
        return Task.CompletedTask;
    }

    public Task<User?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }
}
