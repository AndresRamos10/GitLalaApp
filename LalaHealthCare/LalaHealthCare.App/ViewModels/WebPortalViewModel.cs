using System.Text;
using System.Text.Json;
using System.Windows.Input;
using LalaHealthCare.App.Services;

namespace LalaHealthCare.App.ViewModels;

public class WebPortalViewModel : ViewModelBase, IDisposable
{
    private readonly AppState _appState;
    private readonly ILoggingService _loggingService;
    private readonly string _baseUrl;
    private bool _hasAuthenticated = false;

    private string _userFullName = "Loading...";
    private string _userProfilePictureUrl;
    private bool _isLoading;
    private bool _showUserImage;
    private bool _showDefaultAvatar = true;
    private string _webViewSource;

    public WebPortalViewModel(AppState appState, ILoggingService loggingService) : base(loggingService, appState)
    {
        _appState = appState;
        _loggingService = loggingService;
        _baseUrl = Preferences.Get("WebPortalUrl", "https://lalahealthcareqa-dre0cwdkbafscmc5.eastus2-01.azurewebsites.net/");

        // Initialize commands
        InitializeCommands();

        // Load user info
        LoadUserInfo();

        // Set initial web source
        WebViewSource = _baseUrl;
    }

    #region Properties

    public string UserFullName
    {
        get => _userFullName;
        set => SetProperty(ref _userFullName, value);
    }

    public string UserProfilePictureUrl
    {
        get => _userProfilePictureUrl;
        set => SetProperty(ref _userProfilePictureUrl, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool ShowUserImage
    {
        get => _showUserImage;
        set => SetProperty(ref _showUserImage, value);
    }

    public bool ShowDefaultAvatar
    {
        get => _showDefaultAvatar;
        set => SetProperty(ref _showDefaultAvatar, value);
    }

    public string WebViewSource
    {
        get => _webViewSource;
        set => SetProperty(ref _webViewSource, value);
    }

    public bool HasAuthenticated
    {
        get => _hasAuthenticated;
        set => SetProperty(ref _hasAuthenticated, value);
    }

    #endregion

    #region Commands

    public ICommand CloseCommand { get; private set; }
    public ICommand HomeCommand { get; private set; }
    public ICommand PhoneCommand { get; private set; }
    public ICommand PanicCommand { get; private set; }

    private void InitializeCommands()
    {
        CloseCommand = new Command(async () => await OnCloseAsync());
        HomeCommand = new Command(async () => await OnHomeAsync());
        PhoneCommand = new Command(async () => await OnPhoneAsync());
        PanicCommand = new Command(async () => await OnPanicAsync());
    }

    #endregion

    #region Methods

    private async Task LoadUserInfo()
    {
        try
        {
            if (_appState?.CurrentUser != null)
            {
                UserFullName = _appState.CurrentUser.FullName;

                if (!string.IsNullOrEmpty(_appState.CurrentUser.ProfilePictureUrl))
                {
                    UserProfilePictureUrl = _appState.CurrentUser.ProfilePictureUrl;
                    ShowUserImage = true;
                    ShowDefaultAvatar = false;
                }
            }
            else
            {
                UserFullName = "Guest User";
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error loading user info: {ex.Message}");
            UserFullName = "User";
        }
    }

    public void OnNavigating(string url)
    {
        IsLoading = true;
    }

    public async Task OnNavigatedAsync(WebNavigatedEventArgs args, WebView webView)
    {
        IsLoading = false;

        if (args.Result == WebNavigationResult.Success && !HasAuthenticated)
        {
            await Task.Delay(1500);
            await AuthenticateWithWebPortalAlternativeAsync(webView);
        }
    }

    private async Task OnCloseAsync()
    {
        try
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error closing page: {ex.Message}");
        }
    }

    private async Task OnHomeAsync()
    {
        try
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Navigation error: {ex.Message}");
        }
    }

    private async Task OnPhoneAsync()
    {
        try
        {
            var telephone = Preferences.Default.Get("numberEmergency", "911");

            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(telephone);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Phone dialer is not supported on this device", "OK");
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Limpiar credenciales
            Preferences.Default.Set("username", "");
            Preferences.Default.Set("password", "");
            _appState.Logout();

            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Phone dialer error: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", "Could not make phone call", "OK");
        }
    }

    private async Task OnPanicAsync()
    {
        try
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Navigation to panic button error: {ex.Message}");
        }
    }

    #endregion

    #region Authentication Methods

    private async Task AuthenticateWithWebPortalAlternativeAsync(WebView webView)
    {
        try
        {
            if (_appState?.CurrentUser == null)
            {
                return;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                var authData = new
                {
                    username = _appState.CurrentUser.Username,
                    password = _appState.CurrentUser.Password,
                    fromMobileApp = true,
                    platform = DeviceInfo.Platform.ToString(),
                    appVersion = AppInfo.VersionString,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                var json = JsonSerializer.Serialize(authData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("X-Mobile-App", "true");
                client.DefaultRequestHeaders.Add("X-Mobile-Platform", DeviceInfo.Platform.ToString());

                var response = await client.PostAsync("/api/mobileauth/authenticate", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResponse>(responseContent);

                    if (result.success && !string.IsNullOrEmpty(result.quickLoginUrl))
                    {
                        var fullUrl = $"{_baseUrl}{result.quickLoginUrl}";

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            webView.Source = new UrlWebViewSource { Url = fullUrl };
                        });

                        HasAuthenticated = true;
                    }
                    else
                    {
                        await _loggingService.LogErrorAsync($"Authentication failed: {result.error}");
                    }
                }
                else
                {
                    await _loggingService.LogErrorAsync($"HTTP error: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Alternative authentication error: {ex.Message}");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        // Cleanup if needed
    }

    #endregion

    private class AuthResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
        public string quickLoginToken { get; set; }
        public string quickLoginUrl { get; set; }
        public string redirectUrl { get; set; }
        public string error { get; set; }
        public string message { get; set; }
        public int expiresIn { get; set; }
    }
}