using LalaHealthCare.App.Models;
using System.Text.Json;

namespace LalaHealthCare.App.Services;

public class BiometricCredentialService
{
    private const string BIOMETRIC_ENABLED_KEY = "biometric_enabled";
    private const string BIOMETRIC_USER_KEY = "biometric_user";
    private readonly ILoggingService _loggingService;

    public BiometricCredentialService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<bool> IsBiometricLoginEnabledAsync()
    {
        try
        {
            var enabled = await SecureStorage.GetAsync(BIOMETRIC_ENABLED_KEY);
            return enabled == "true";
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error checking biometric login status", ex);
            return false;
        }
    }

    public async Task EnableBiometricLoginAsync(string username)
    {
        try
        {
            await SecureStorage.SetAsync(BIOMETRIC_ENABLED_KEY, "true");
            await SecureStorage.SetAsync(BIOMETRIC_USER_KEY, username);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error enabling biometric login", ex);
            throw;
        }
    }

    public async Task DisableBiometricLoginAsync()
    {
        try
        {
            SecureStorage.Remove(BIOMETRIC_ENABLED_KEY);
            SecureStorage.Remove(BIOMETRIC_USER_KEY);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error disabling biometric login", ex);
        }
    }

    public async Task<string?> GetBiometricUsernameAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(BIOMETRIC_USER_KEY);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error getting biometric username", ex);
            return null;
        }
    }
}
