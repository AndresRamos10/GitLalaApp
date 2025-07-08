using System.ComponentModel;
using System.Runtime.CompilerServices;
using LalaHealthCare.App.Services;

namespace LalaHealthCare.App.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;
    public readonly AppState _appState;
    private readonly ILoggingService _loggingService;
    private bool _isAuthenticated = false;

    protected ViewModelBase(ILoggingService loggingService, AppState appState)
    {
        _loggingService = loggingService;
        _appState = appState;
    }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    protected async Task ExecuteAsync(Func<Task> operation, [CallerMemberName] string? operationName = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _loggingService.LogInformationAsync($"Starting operation: {operationName}");
            await operation();
            await _loggingService.LogInformationAsync($"Completed operation: {operationName}");
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, $"Error in {operationName}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, [CallerMemberName] string? operationName = null)
    {
        if (IsBusy)
            return default;

        try
        {
            IsBusy = true;
            await _loggingService.LogInformationAsync($"Starting operation: {operationName}");
            var result = await operation();
            await _loggingService.LogInformationAsync($"Completed operation: {operationName}");
            return result;
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex, $"Error in {operationName}");
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task HandleErrorAsync(Exception exception, string context)
    {
        await _loggingService.LogErrorAsync($"Error in {GetType().Name}: {context}", exception, new Dictionary<string, object>
        {
            { "ViewModel", GetType().Name },
            { "Context", context },
            { "Timestamp", DateTime.UtcNow }
        });

        // Aquí podrías mostrar un mensaje al usuario
        await ShowErrorToUserAsync(exception.Message);
    }

    protected virtual Task ShowErrorToUserAsync(string message)
    {
        // Este método será sobrescrito en las implementaciones específicas
        // para mostrar errores al usuario (Snackbar, Dialog, etc.)
        return Task.CompletedTask;
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task CleanupAsync()
    {
        return Task.CompletedTask;
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}