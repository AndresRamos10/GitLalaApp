namespace LalaHealthCare.App.Services;
public class SecureCredentialService : ISecureCredentialService
{
    private const string USERNAME_KEY = "biometric_username";
    private const string PASSWORD_KEY = "biometric_password";
    private const string CREDENTIALS_HASH_KEY = "credentials_hash";
    private readonly ILoggingService _loggingService;

    public SecureCredentialService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task SaveCredentialsAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username and password cannot be empty");
            }

            // Encriptar credenciales antes de guardar
            var encryptedUsername = EncryptData(username);
            var encryptedPassword = EncryptData(password);

            // Crear hash para validación de integridad
            var credentialsHash = CreateCredentialsHash(username, password);

            await SecureStorage.SetAsync(USERNAME_KEY, encryptedUsername);
            await SecureStorage.SetAsync(PASSWORD_KEY, encryptedPassword);
            await SecureStorage.SetAsync(CREDENTIALS_HASH_KEY, credentialsHash);

        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error saving credentials: {ex.Message}");
            throw;
        }
    }

    public async Task<(string username, string password)> GetSavedCredentialsAsync()
    {
        try
        {
            var encryptedUsername = await SecureStorage.GetAsync(USERNAME_KEY);
            var encryptedPassword = await SecureStorage.GetAsync(PASSWORD_KEY);

            if (string.IsNullOrEmpty(encryptedUsername) || string.IsNullOrEmpty(encryptedPassword))
            {
                return (null, null);
            }

            // Desencriptar credenciales
            var username = DecryptData(encryptedUsername);
            var password = DecryptData(encryptedPassword);

            // Validar integridad
            if (!await ValidateStoredCredentialsAsync())
            {
                await ClearCredentialsAsync();
                return (null, null);
            }

            return (username, password);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error retrieving credentials: {ex.Message}");
            return (null, null);
        }
    }

    public async Task<bool> HasSavedCredentialsAsync()
    {
        try
        {
            var username = await SecureStorage.GetAsync(USERNAME_KEY);
            var password = await SecureStorage.GetAsync(PASSWORD_KEY);

            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error checking saved credentials: {ex.Message}");
            return false;
        }
    }

    public async Task ClearCredentialsAsync()
    {
        try
        {
            SecureStorage.Remove(USERNAME_KEY);
            SecureStorage.Remove(PASSWORD_KEY);
            SecureStorage.Remove(CREDENTIALS_HASH_KEY);
        }
        catch (Exception ex)
        {
           await _loggingService.LogErrorAsync($"Error clearing credentials: {ex.Message}");
        }
    }

    public async Task<bool> ValidateStoredCredentialsAsync()
    {
        try
        {
            var storedHash = await SecureStorage.GetAsync(CREDENTIALS_HASH_KEY);
            if (string.IsNullOrEmpty(storedHash))
                return false;

            var (username, password) = await GetSavedCredentialsAsync();
            if (username == null || password == null)
                return false;

            var calculatedHash = CreateCredentialsHash(username, password);
            return storedHash == calculatedHash;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync($"Error validating credentials: {ex.Message}");
            return false;
        }
    }

    private string EncryptData(string data)
    {
        // Implementación básica de encriptación
        // En producción, usar algoritmos más robustos
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return Convert.ToBase64String(bytes);
    }

    private string DecryptData(string encryptedData)
    {
        try
        {
            var bytes = Convert.FromBase64String(encryptedData);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }

    private string CreateCredentialsHash(string username, string password)
    {
        var combined = $"{username}:{password}:{DateTime.UtcNow:yyyy-MM-dd}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

