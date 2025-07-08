using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.App.Services;

public class AppState
{
    public User? CurrentUser { get; set; }
    public string Token { get; set; }
    public event Action? OnChange;

    public void SetUser(User? user, string? token)
    {
        CurrentUser = user;
        Token = token;
        NotifyStateChanged();
    }

    public void Logout()
    {
        // Limpiar el usuario actual
        CurrentUser = null;       
        Token = string.Empty;
        // Notificar que el estado ha cambiado
        NotifyStateChanged();
    }   

    private void NotifyStateChanged() => OnChange?.Invoke();
}
