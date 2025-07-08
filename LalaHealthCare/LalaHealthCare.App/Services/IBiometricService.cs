using LalaHealthCare.App.Models;

namespace LalaHealthCare.App.Services;

public interface IBiometricService
{
    Task<bool> IsAvailableAsync();
    Task<BiometricAuthenticationResult> AuthenticateAsync(string reason);
    Task<bool> HasEnrolledBiometricsAsync();
    Task<BiometricAuthenticationType> GetAuthenticationType();
}
