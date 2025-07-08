using LalaHealthCare.DataAccess.Models;
using static LalaHealthCare.DataAccess.Models.AuthDtos;

namespace LalaHealthCare.Business.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> BiometricLoginAsync(BiometricLoginRequest request);
    Task LogoutAsync();
    Task<User?> GetCurrentUserAsync();
    bool IsAuthenticated { get; }
}
