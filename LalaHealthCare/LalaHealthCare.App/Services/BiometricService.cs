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
#if IOS
            // En iOS, verificar primero si tenemos el permiso de Face ID
            var status = await GetAvailabilityAsync();
            await _loggingService.LogInformationAsync($"iOS Biometric availability status: {status}");
            
            return status == FingerprintAvailability.Available;
#else
            return await CrossFingerprint.Current.IsAvailableAsync();
#endif
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
            var availability = await GetAvailabilityAsync();
            await _loggingService.LogInformationAsync($"Biometric enrollment status: {availability}");
            return availability == FingerprintAvailability.Available;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error checking enrolled biometrics", ex);
            return false;
        }
    }

    private async Task<FingerprintAvailability> GetAvailabilityAsync()
    {
        try
        {
            var availability = await CrossFingerprint.Current.GetAvailabilityAsync();

#if IOS
            // Log adicional para iOS
            await _loggingService.LogInformationAsync($"Raw iOS availability: {availability}");
            
            // En iOS, si obtenemos Unknown, intentar forzar una verificación
            if (availability == FingerprintAvailability.Unknown)
            {
                // Intentar obtener el tipo de autenticación para forzar la inicialización
                var authType = await CrossFingerprint.Current.GetAuthenticationTypeAsync();
                await _loggingService.LogInformationAsync($"iOS Auth Type: {authType}");
                
                // Volver a verificar disponibilidad
                availability = await CrossFingerprint.Current.GetAvailabilityAsync();
            }
#endif

            return availability;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error in GetAvailabilityAsync", ex);
            return FingerprintAvailability.Unknown;
        }
    }

    public async Task<BiometricAuthenticationResult> AuthenticateAsync(string reason)
    {
        try
        {
            // Verificar disponibilidad
            var availability = await GetAvailabilityAsync();

            if (availability != FingerprintAvailability.Available)
            {
                await _loggingService.LogWarningAsync($"Biometric not available: {availability}");
                return new BiometricAuthenticationResult
                {
                    IsSuccess = false,
                    Status = GetStatusFromAvailability(availability),
                    ErrorMessage = GetErrorMessageFromAvailability(availability)
                };
            }

            // Configurar la solicitud de autenticación
            var request = new AuthenticationRequestConfiguration(
                title: "CareNote360",
                reason: reason)
            {
                AllowAlternativeAuthentication = true,
                CancelTitle = "Cancelar",
                FallbackTitle = "Usar contraseña"
            };

//#if IOS
//            // Para iOS, asegurar que usamos las configuraciones correctas
//            request.UseDialog = false; // En iOS no usar diálogo personalizado
//#endif

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
                await _loggingService.LogWarningAsync($"Biometric authentication failed: {result.Status} - {result.ErrorMessage}");

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
                    ErrorMessage = result.ErrorMessage ?? "Autenticación fallida"
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

            await _loggingService.LogInformationAsync($"Authentication type detected: {type}");

            return type switch
            {
                AuthenticationType.Fingerprint => BiometricAuthenticationType.Fingerprint,
                AuthenticationType.Face => BiometricAuthenticationType.Face,
                AuthenticationType.None => BiometricAuthenticationType.None,
                _ => BiometricAuthenticationType.Unknown
            };
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error getting authentication type", ex);
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