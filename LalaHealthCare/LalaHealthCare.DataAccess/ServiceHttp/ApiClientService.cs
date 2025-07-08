using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LalaHealthCare.DataAccess.ServiceHttp;

namespace LalaHealthCare.DataAccess.ServicesHttp
{
    public enum AuthenticationType
    {
        Basic,
        Bearer
    }

    public class ApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _username;
        private readonly string _password;
        private readonly ITokenProvider _tokenProvider;

        public ApiClientService(HttpClient httpClient, ITokenProvider tokenProvider, string username, string password)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;
            _username = username;
            _password = password;

            // Configurar JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Configurar headers por defecto
            ConfigureDefaultHeaders();
        }

        private void ConfigureDefaultHeaders()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-Mobile-App", "true");
            _httpClient.DefaultRequestHeaders.Add("X-Platform", DeviceInfo.Platform.ToString());
            _httpClient.DefaultRequestHeaders.Add("X-App-Version", AppInfo.VersionString);
        }

        /// <summary>
        /// Configura el header de autorización según el tipo seleccionado
        /// </summary>
        private void SetAuthorizationHeader(AuthenticationType type)
        {
            switch (type)
            {
                case AuthenticationType.Basic:
                    SetBasicAuthHeader();
                    break;
                case AuthenticationType.Bearer:
                    SetBearerAuthHeader();
                    break;
            }
        }

        private void SetBasicAuthHeader()
        {
            var basicAuthBytes = Encoding.UTF8.GetBytes($"{_username}:{_password}");
            var basicAuthValue = Convert.ToBase64String(basicAuthBytes);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
        }

        private void SetBearerAuthHeader()
        {
            // Obtener el token desde ITokenProvider
            var bearerToken = _tokenProvider.GetAccessToken();

            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            else
            {
                throw new InvalidOperationException("Bearer token not found. Please ensure user is authenticated.");
            }
        }

        /// <summary>
        /// GET request genérico
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint, AuthenticationType authenticationType = AuthenticationType.Bearer)
        {
            try
            {
                SetAuthorizationHeader(authenticationType);

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Unauthorized access. Please check your credentials.");
                }

                // Log del error para debug
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status {response.StatusCode}: {errorContent}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Network error. Please check your connection.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Request timeout. Please try again.", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// POST request genérico
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, AuthenticationType authenticationType = AuthenticationType.Bearer)
        {
            try
            {
                SetAuthorizationHeader(authenticationType);

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Unauthorized access. Please check your credentials.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status {response.StatusCode}: {errorContent}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// PUT request genérico
        /// </summary>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, AuthenticationType authenticationType = AuthenticationType.Bearer)
        {
            try
            {
                SetAuthorizationHeader(authenticationType);

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Unauthorized access. Please check your credentials.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status {response.StatusCode}: {errorContent}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// DELETE request
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint, AuthenticationType authenticationType = AuthenticationType.Bearer)
        {
            try
            {
                SetAuthorizationHeader(authenticationType);

                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log del error si es necesario
                return false;
            }
        }

        /// <summary>
        /// Método para cambiar el tipo de autenticación en tiempo de ejecución
        /// </summary>
        public void ChangeAuthenticationType(AuthenticationType newAuthType)
        {
            // Ya no necesitamos pasar el bearerToken porque se obtiene de AppState
        }
    }
}