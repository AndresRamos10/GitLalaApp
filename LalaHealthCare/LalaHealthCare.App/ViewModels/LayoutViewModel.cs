using LalaHealthCare.App.Services;
using MudBlazor;
using System.Windows.Input;

namespace LalaHealthCare.App.ViewModels;

public class LayoutViewModel: ViewModelBase
{
    private readonly AppState _appState;
    private readonly ILoggingService _loggingService;
    private readonly ISnackbar _snackbar;
    public LayoutViewModel(ILoggingService loggingService, ISnackbar snackbar, AppState appState) :base(loggingService, appState)
    {
        _appState = appState;
        _snackbar = snackbar;
        _loggingService = loggingService;
        ShowFastOptionsCommand = new Command(() => ShowFastOptions = !ShowFastOptions);
        Emergency911Command = new Command(async () => await HandleEmergency911Async());
        PanicButtonCommand = new Command(async () => await HandlePanicButtonAsync());
    }

    private bool _showFastOptions = true;
    public string UserFullName => _appState.CurrentUser?.FullName ?? "Nurse";
    public string UserProfilePictureUrl => _appState.CurrentUser?.ProfilePictureUrl ?? string.Empty;

    #region Properties
    public bool ShowFastOptions
    {
        get => _showFastOptions;
        set => SetProperty(ref _showFastOptions, value);
    }
    #endregion

    #region Commands
    public ICommand ShowFastOptionsCommand { get; }
    public ICommand Emergency911Command { get; }
    public ICommand PanicButtonCommand { get; }

    #endregion

    private async Task HandleEmergency911Async()
    {
        try
        {
            // TODO: Implementar llamada real al 911
            _snackbar.Add("Llamando al 911...", Severity.Error);
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error calling 911", ex);
        }
    }

    private async Task HandlePanicButtonAsync()
    {
        try
        {
            // TODO: Implementar alerta de pánico real
            _snackbar.Add("Alerta de pánico enviada", Severity.Warning);
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error sending panic alert", ex);
        }
    }
}
