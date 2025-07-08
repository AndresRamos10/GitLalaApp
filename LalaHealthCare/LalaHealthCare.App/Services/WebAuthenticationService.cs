using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LalaHealthCare.App.Services;

public class WebAuthenticationService
//{
//    private readonly HttpClient _httpClient;
//    private readonly ILogger<WebAuthenticationService> _logger;
//    private readonly AppState _appState;
//    private readonly ISecureStorage _secureStorage;

//    public string BaseUrl { get; } = "https://lalahealthcareqa-dre0cwdkbafscmc5.eastus2-01.azurewebsites.net";

//    private const string AuthTokenKey = "web_auth_token";
//    private const string SessionCookieKey = "web_session_cookie";

//    public WebAuthenticationService(
//        HttpClient httpClient,
//        ILogger<WebAuthenticationService> logger,
//        AppState appState,
//        ISecureStorage secureStorage)
//    {
//        _httpClient = httpClient;
//        _logger = logger;
//        _appState = appState;
//        _secureStorage = secureStorage;

//        _httpClient.BaseAddress = new Uri(BaseUrl);
//    }

//    /// <summary>
//    /// Obtiene un token de autenticación para el portal web usando las credenciales del usuario actual
//    /// </summary>
//    public async Task<string> GetAuthenticationTokenAsync()
//    {
//        try
//        {
//            // Verificar si ya tenemos un token guardado
//            var savedToken = await _secureStorage.GetAsync(AuthTokenKey);
//            if (!string.IsNullOrEmpty(savedToken))
//            {
//                // TODO: Verificar si el token sigue siendo válido
//                return savedToken;
//            }

//            // Si no hay token, generar uno nuevo
//            if (_appState.CurrentUser != null)
//            {
//                var authRequest = new
//                {
//                    username = _appState.CurrentUser.Username,
//                    email = _appState.CurrentUser.Email,
//                    userId = _appState.CurrentUser.Id,
//                    role = _appState.CurrentUser.Role,
//                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
//                };

//                // Crear un token temporal (en producción, esto debería venir del servidor)
//                var token = Convert.ToBase64String(
//                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authRequest))
//                );

//                await _secureStorage.SetAsync(AuthTokenKey, token);
//                return token;
//            }

//            throw new InvalidOperationException("User is not authenticated");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting authentication token");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Construye la URL de autenticación para el portal web
//    /// </summary>
//    public async Task<string> GetAuthenticatedUrlAsync(string targetPath = "/")
//    {
//        try
//        {
//            var token = await GetAuthenticationTokenAsync();
//            var encodedToken = Uri.EscapeDataString(token);

//            // Construir URL con parámetros de autenticación
//            var separator = targetPath.Contains("?") ? "&" : "?";
//            var authUrl = $"{BaseUrl}{targetPath}{separator}authToken={encodedToken}&source=mobile";

//            _logger.LogInformation($"Generated authenticated URL: {authUrl}");
//            return authUrl;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error generating authenticated URL");
//            return $"{BaseUrl}/login?returnUrl={Uri.EscapeDataString(targetPath)}";
//        }
//    }

//    /// <summary>
//    /// Invalida el token de autenticación actual
//    /// </summary>
//    public async Task LogoutAsync()
//    {
//        try
//        {
//            _secureStorage.Remove(AuthTokenKey);
//            _secureStorage.Remove(SessionCookieKey);

//            // Notificar al servidor web sobre el logout si es necesario
//            // await _httpClient.PostAsync("/api/auth/logout", null);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error during web logout");
//        }
//    }

//    /// <summary>
//    /// Almacena cookies de sesión del portal web
//    /// </summary>
//    public async Task SaveSessionCookieAsync(string cookieValue)
//    {
//        if (!string.IsNullOrEmpty(cookieValue))
//        {
//            await _secureStorage.SetAsync(SessionCookieKey, cookieValue);
//        }
//    }

//    /// <summary>
//    /// Obtiene las cookies de sesión guardadas
//    /// </summary>
//    public async Task<string> GetSessionCookieAsync()
//    {
//        return await _secureStorage.GetAsync(SessionCookieKey) ?? string.Empty;
//    }

//    /// <summary>
//    /// Verifica si el usuario tiene una sesión válida en el portal web
//    /// </summary>
//    public async Task<bool> HasValidWebSessionAsync()
//    {
//        try
//        {
//            var token = await _secureStorage.GetAsync(AuthTokenKey);
//            var cookie = await _secureStorage.GetAsync(SessionCookieKey);

//            return !string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(cookie);
//        }
//        catch
//        {
//            return false;
//        }
//    }
//}

{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebAuthenticationService> _logger;
    private readonly AppState _appState;
    private readonly ISecureStorage _secureStorage;
    private CookieContainer _cookieContainer;

    public string BaseUrl { get; } = "https://lalahealthcareqa-dre0cwdkbafscmc5.eastus2-01.azurewebsites.net";

    private const string AuthTokenKey = "web_auth_token";
    private const string SessionCookieKey = "web_session_cookie";
    private const string SessionKey = "web_session";
    public WebAuthenticationService(
        HttpClient httpClient,
        ILogger<WebAuthenticationService> logger,
        AppState appState,
        ISecureStorage secureStorage)
    {
        _httpClient = httpClient;
        _logger = logger;
        _appState = appState;
        _secureStorage = secureStorage;
        _cookieContainer = new CookieContainer();

        // Configurar HttpClient con cookies
        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true
        };
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    /// <summary>
    /// Intenta autenticar usando las credenciales del usuario actual
    /// mediante POST al formulario de login del sitio web
    /// </summary>
    public async Task<bool> AuthenticateInWebPortalAsync()
    {
        try
        {
            if (_appState.CurrentUser == null)
            {
                return false;
            }

            // Primero, obtener la página de login para obtener el token antiforgery
            var loginPageResponse = await _httpClient.GetAsync("/Account/Login");
            if (!loginPageResponse.IsSuccessStatusCode)
            {
                return false;
            }

            var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();
            var antiforgeryToken = ExtractAntiforgeryToken(loginPageContent);

            // Preparar los datos del formulario
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Username", _appState.CurrentUser.Username),
                new KeyValuePair<string, string>("Password", _appState.CurrentUser.Password), // En producción, almacenar de forma segura
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("RememberMe", "false")
            });

            // Enviar el formulario de login
            var loginResponse = await _httpClient.PostAsync("/Account/Login", formData);

            // Verificar si el login fue exitoso
            if (loginResponse.StatusCode == HttpStatusCode.Redirect ||
                loginResponse.StatusCode == HttpStatusCode.OK)
            {
                // Guardar las cookies de sesión
                await SaveWebCookiesAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating in web portal");
            return false;
        }
    }

    private string ExtractAntiforgeryToken(string html)
    {
        // Buscar el token antiforgery en el HTML
        var tokenStart = html.IndexOf("__RequestVerificationToken\" value=\"");
        if (tokenStart == -1) return string.Empty;

        tokenStart += 35; // Longitud del string de búsqueda
        var tokenEnd = html.IndexOf("\"", tokenStart);

        return html.Substring(tokenStart, tokenEnd - tokenStart);
    }

    private async Task SaveWebCookiesAsync()
    {
        var cookies = _cookieContainer.GetCookies(new Uri(BaseUrl));
        var cookieData = new Dictionary<string, string>();

        foreach (Cookie cookie in cookies)
        {
            cookieData[cookie.Name] = cookie.Value;
        }

        var cookieJson = JsonSerializer.Serialize(cookieData);
        await _secureStorage.SetAsync("web_cookies", cookieJson);
    }

    public async Task<Dictionary<string, string>> GetSavedCookiesAsync()
    {
        var cookieJson = await _secureStorage.GetAsync("web_cookies");
        if (string.IsNullOrEmpty(cookieJson))
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(cookieJson)
               ?? new Dictionary<string, string>();
    }





    /// <summary>
    /// Obtiene la URL del portal web
    /// Por ahora retorna la URL base, en el futuro puede incluir tokens
    /// </summary>
    public async Task<string> GetAuthenticatedUrlAsync(string targetPath = "/")
    {
        try
        {
            // Asegurar que el path comience con /
            if (!targetPath.StartsWith("/"))
                targetPath = "/" + targetPath;

            var url = $"{BaseUrl}{targetPath}";

            _logger.LogInformation($"Generated URL: {url}");
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating URL");
            return BaseUrl;
        }
    }

    /// <summary>
    /// Guarda información de sesión
    /// </summary>
    public async Task SaveSessionInfoAsync(string sessionInfo)
    {
        try
        {
            if (!string.IsNullOrEmpty(sessionInfo))
            {
                await _secureStorage.SetAsync(SessionKey, sessionInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving session info");
        }
    }

    /// <summary>
    /// Obtiene información de sesión guardada
    /// </summary>
    public async Task<string> GetSessionInfoAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(SessionKey) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session info");
            return string.Empty;
        }
    }

    /// <summary>
    /// Limpia la información de sesión
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            _secureStorage.Remove(SessionKey);
            _logger.LogInformation("Web session cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    /// <summary>
    /// Verifica si hay una sesión guardada
    /// </summary>
    public async Task<bool> HasSessionAsync()
    {
        try
        {
            var session = await GetSessionInfoAsync();
            return !string.IsNullOrEmpty(session);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene información del usuario actual para mostrar en el portal
    /// </summary>
    public Dictionary<string, string> GetCurrentUserInfo()
    {
        var userInfo = new Dictionary<string, string>();

        if (_appState.CurrentUser != null)
        {
            userInfo["userId"] = _appState.CurrentUser.Id.ToString();
            userInfo["clinitianId"] = _appState.CurrentUser.ClinicianId;
            userInfo["username"] = _appState.CurrentUser.Username;
            userInfo["fullName"] = _appState.CurrentUser.FullName;
            userInfo["email"] = _appState.CurrentUser.Email ?? "";
            userInfo["role"] = _appState.CurrentUser.Role ?? "";
        }

        return userInfo;
    }
}