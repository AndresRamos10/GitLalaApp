using LalaHealthCare.App.Models;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Threading.Tasks;

namespace LalaHealthCare.App.Services;

public class BiometricService : IBiometricService
{
    private readonly ILoggingService _loggingService;

    public BiometricService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await CrossFingerprint.Current.IsAvailableAsync();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error checking biometric availability", ex);
            return false;
        }
    }

    public async Task<bool> HasEnrolledBiometricsAsync()
    {
        try
        {
            var availability = await CrossFingerprint.Current.GetAvailabilityAsync();
            return availability == FingerprintAvailability.Available;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error checking enrolled biometrics", ex);
            return false;
        }
    }

    public async Task<BiometricAuthenticationResult> AuthenticateAsync(string reason)
    {
        try
        {
            // Verificar disponibilidad
            var availability = await CrossFingerprint.Current.GetAvailabilityAsync();

            if (availability != FingerprintAvailability.Available)
            {
                return new BiometricAuthenticationResult
                {
                    IsSuccess = false,
                    Status = GetStatusFromAvailability(availability),
                    ErrorMessage = GetErrorMessageFromAvailability(availability)
                };
            }

            // Configurar la solicitud de autenticación ajuste Andres
            var request = new AuthenticationRequestConfiguration("Prove you have fingers!", "Because without it you can't have access");
            //reason: reason,
            ////cancel: "Cancelar",
            ////fallback: "Usar contraseña",
            //title: "Demasiado rápido, intenta de nuevo")
            //{
            //    AllowAlternativeAuthentication = true,
            //    CancelTitle = "Cancelar autenticación",
            //    FallbackTitle = "Usar método alternativo"
            //};

            // Realizar autenticación
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                await _loggingService.LogInformationAsync("Biometric authentication successful");

                return new BiometricAuthenticationResult
                {
                    IsSuccess = true,
                    Status = BiometricAuthenticationStatus.Succeeded
                };
            }
            else
            {
                var status = result.Status switch
                {
                    FingerprintAuthenticationResultStatus.Canceled => BiometricAuthenticationStatus.Cancelled,
                    FingerprintAuthenticationResultStatus.Failed => BiometricAuthenticationStatus.Failed,
                    FingerprintAuthenticationResultStatus.TooManyAttempts => BiometricAuthenticationStatus.Failed,
                    _ => BiometricAuthenticationStatus.Unknown
                };

                return new BiometricAuthenticationResult
                {
                    IsSuccess = false,
                    Status = status,
                    ErrorMessage = result.ErrorMessage
                };
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error during biometric authentication", ex);

            return new BiometricAuthenticationResult
            {
                IsSuccess = false,
                Status = BiometricAuthenticationStatus.Unknown,
                ErrorMessage = "Error inesperado durante la autenticación"
            };
        }
    }

    public async Task<BiometricAuthenticationType> GetAuthenticationType()
    {
        try
        {
            var type = await CrossFingerprint.Current.GetAuthenticationTypeAsync();

            return type switch
            {
                AuthenticationType.Fingerprint => BiometricAuthenticationType.Fingerprint,
                AuthenticationType.Face => BiometricAuthenticationType.Face,
                //AuthenticationType.Iris => BiometricAuthenticationType.Iris,
                AuthenticationType.None => BiometricAuthenticationType.None,
                _ => BiometricAuthenticationType.Unknown
            };
        }
        catch (Exception ex)
        {
            _loggingService.LogErrorAsync("Error getting authentication type", ex).Wait();
            return BiometricAuthenticationType.Unknown;
        }
    }

    private BiometricAuthenticationStatus GetStatusFromAvailability(FingerprintAvailability availability)
    {
        return availability switch
        {
            FingerprintAvailability.Available => BiometricAuthenticationStatus.Succeeded,
            FingerprintAvailability.NoImplementation => BiometricAuthenticationStatus.NotAvailable,
            FingerprintAvailability.NoPermission => BiometricAuthenticationStatus.PermissionDenied,
            FingerprintAvailability.NoSensor => BiometricAuthenticationStatus.NotAvailable,
            FingerprintAvailability.NoFingerprint => BiometricAuthenticationStatus.NotEnrolled,
            FingerprintAvailability.Unknown => BiometricAuthenticationStatus.Unknown,
            _ => BiometricAuthenticationStatus.Unknown
        };
    }

    private string GetErrorMessageFromAvailability(FingerprintAvailability availability)
    {
        return availability switch
        {
            FingerprintAvailability.NoImplementation => "La autenticación biométrica no está implementada en este dispositivo",
            FingerprintAvailability.NoPermission => "Se requiere permiso para usar la autenticación biométrica",
            FingerprintAvailability.NoSensor => "Este dispositivo no tiene sensor biométrico",
            FingerprintAvailability.NoFingerprint => "No hay datos biométricos registrados. Por favor, configura tu huella digital o Face ID en la configuración del dispositivo",
            FingerprintAvailability.Unknown => "Estado de autenticación biométrica desconocido",
            _ => "La autenticación biométrica no está disponible"
        };
    }
}
