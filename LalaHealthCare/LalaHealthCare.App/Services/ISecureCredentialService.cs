namespace LalaHealthCare.App.Services;

public interface ISecureCredentialService
{
    Task SaveCredentialsAsync(string username, string password);
    Task<(string username, string password)> GetSavedCredentialsAsync();
    Task<bool> HasSavedCredentialsAsync();
    Task ClearCredentialsAsync();
    Task<bool> ValidateStoredCredentialsAsync();
}
