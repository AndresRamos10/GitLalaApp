using System.Windows.Input;
using LalaHealthCare.Business.Services;
using LalaHealthCare.App.Services;
using LalaHealthCare.App.Extensions;
using MudBlazor;
using LalaHealthCare.App.Models;
using static LalaHealthCare.DataAccess.Models.AuthDtos;

namespace LalaHealthCare.App.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;    
    private readonly ISnackbar _snackbar;
    private readonly ILoggingService _loggingService;
    private readonly IBiometricService _biometricService;
    private readonly BiometricCredentialService _biometricCredentialService;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _selectedLanguage = "en";
    private bool _isPasswordVisible = false;

    public LoginViewModel(
        IAuthenticationService authService,
        ISnackbar snackbar,
        AppState appState,
        ILoggingService loggingService,
        IBiometricService biometricService,
        BiometricCredentialService biometricCredentialService) : base(loggingService, appState)
    {
        _authService = authService;
        _snackbar = snackbar;
        _loggingService = loggingService;
        _biometricService = biometricService;
        _biometricCredentialService = biometricCredentialService;
        

        Title = "Login";        

        // Inicializar comandos
        LoginCommand = new AsyncCommand(async () => await LoginAsync());
        BiometricLoginCommand = new AsyncCommand(async () => await BiometricLoginAsync());
        TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
    }

    #region Properties

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public string PasswordInputType => IsPasswordVisible ? "text" : "password";
    public string PasswordVisibilityIcon => IsPasswordVisible ? Icons.Material.Filled.Visibility : Icons.Material.Filled.VisibilityOff;

    #endregion

    #region Commands

    public ICommand LoginCommand { get; }
    public ICommand BiometricLoginCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }

    #endregion

    #region Methods

    private async Task LoginAsync()
    {
        //Username = "nursenotes@nursenotes.com";
        //Password = "123456";
        if (!ValidateInput())
        {
            return;
        }

        try
        {
            IsBusy = true;

            var loginRequest = new LoginRequest
            {
                Username = Username,
                Password = Password
            };

            var response = await _authService.LoginAsync(loginRequest);
            
            if (response.Success && response.User != null)
            {
                IsAuthenticated = _authService.IsAuthenticated;
                response.User.Password = Password;
                _appState.SetUser(response.User, response.Token);
                _snackbar.Add($"¡Bienvenido {response.User.FullName}!", Severity.Success);

                // Ofrecer habilitar biometría si está disponible y no está habilitada
                await CheckAndOfferBiometricSetupAsync(response.User.Username);
            }
            else
            {
                _snackbar.Add(response.ErrorMessage ?? "Error al iniciar sesión", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error during login", ex);
            _snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CheckAndOfferBiometricSetupAsync(string username)
    {
        try
        {
            // Verificar si biometría está disponible
            var isAvailable = await _biometricService.IsAvailableAsync();
            if (!isAvailable) return;

            // Verificar si ya está habilitada
            var isEnabled = await _biometricCredentialService.IsBiometricLoginEnabledAsync();
            if (isEnabled) return;

            // Verificar si hay biometría enrollada
            var hasEnrolled = await _biometricService.HasEnrolledBiometricsAsync();
            if (!hasEnrolled) return;

            // Obtener tipo de autenticación
            var authType = await _biometricService.GetAuthenticationType();
            var authTypeName = authType switch
            {
                BiometricAuthenticationType.Fingerprint => "huella digital",
                BiometricAuthenticationType.Face => "Face ID",
                _ => "biometría"
            };

            // Preguntar al usuario si quiere habilitar
            var result = await Application.Current!.MainPage!.DisplayAlert(
                "Habilitar Acceso Biométrico",
                $"¿Deseas habilitar el acceso con {authTypeName} para futuros inicios de sesión?",
                "Sí, habilitar",
                "No, gracias"
            );

            if (result)
            {
                await _biometricCredentialService.EnableBiometricLoginAsync(username);
                _snackbar.Add($"Acceso con {authTypeName} habilitado", Severity.Success);
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error offering biometric setup", ex);
        }
    }

    private async Task BiometricLoginAsync()
    {
        try
        {
            IsBusy = true;

            // Verificar si biometría está disponible
            var isAvailable = await _biometricService.IsAvailableAsync();
            if (!isAvailable)
            {
                await LoginAsync();
                return;
            }

            // Verificar si el usuario tiene biometría habilitada
            var isBiometricEnabled = await _biometricCredentialService.IsBiometricLoginEnabledAsync();
            if (!isBiometricEnabled)
            {
                await LoginAsync();
                // Si no está habilitada, preguntar si quiere habilitarla después del primer login exitoso
                _snackbar.Add("Primero inicia sesión con tu contraseña para habilitar el acceso biométrico", Severity.Info);
                return;
            }

            // Obtener el usuario asociado con la biometría
            var savedUsername = await _biometricCredentialService.GetBiometricUsernameAsync();
            if (string.IsNullOrEmpty(savedUsername))
            {
                _snackbar.Add("No hay usuario asociado con la autenticación biométrica", Severity.Warning);
                await _biometricCredentialService.DisableBiometricLoginAsync();
                return;
            }

            // Realizar autenticación biométrica
            var biometricResult = await _biometricService.AuthenticateAsync("Inicia sesión en LalaHealthCare");

            if (biometricResult.IsSuccess)
            {
                // Simular login con el usuario guardado
                var request = new BiometricLoginRequest
                {
                    DeviceId = GetDeviceId(),
                    BiometricData = "authenticated"
                };

                var response = await _authService.BiometricLoginAsync(request);

                if (response.Success && response.User != null)
                {
                    IsAuthenticated = _authService.IsAuthenticated;
                    _appState.SetUser(response.User, response.Token);
                    _snackbar.Add($"¡Bienvenido {response.User.FullName}!", Severity.Success);
                }
                else
                {
                    _snackbar.Add("Error en autenticación biométrica", Severity.Error);
                    await _biometricCredentialService.DisableBiometricLoginAsync();
                }
            }
            else
            {
                // Manejar diferentes estados de error
                switch (biometricResult.Status)
                {
                    case BiometricAuthenticationStatus.Cancelled:
                        _snackbar.Add("Autenticación cancelada", Severity.Info);
                        break;
                    case BiometricAuthenticationStatus.NotEnrolled:
                        _snackbar.Add("No hay datos biométricos registrados en este dispositivo", Severity.Warning);
                        break;
                    case BiometricAuthenticationStatus.PermissionDenied:
                        _snackbar.Add("Se requieren permisos para usar la autenticación biométrica", Severity.Warning);
                        break;
                    default:
                        _snackbar.Add(biometricResult.ErrorMessage ?? "Error en autenticación biométrica", Severity.Error);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error during biometric login", ex);
            _snackbar.Add("Error en autenticación biométrica", Severity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            _snackbar.Add("Por favor complete todos los campos", Severity.Warning);
            return false;
        }
        return true;
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private string GetDeviceId()
    {
        try
        {
            // En una app real, esto obtendría el ID único del dispositivo
            return $"{DeviceInfo.Current.Platform}-{DeviceInfo.Current.Model}-001";
        }
        catch
        {
            return "device-001";
        }
    }

    protected override async Task ShowErrorToUserAsync(string message)
    {
        _snackbar.Add(message, Severity.Error);
        await Task.CompletedTask;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // Limpiar campos
        Username = string.Empty;
        Password = string.Empty;
    }

    #endregion
}