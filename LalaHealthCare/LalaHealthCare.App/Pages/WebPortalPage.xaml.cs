using LalaHealthCare.App.Services;
using LalaHealthCare.App.ViewModels;

namespace LalaHealthCare.App.Pages;

public partial class WebPortalPage : ContentPage
{
    private WebPortalViewModel _viewModel;

    public WebPortalPage()
    {
        InitializeComponent();

        // Obtener servicios
        var serviceProvider = Application.Current.MainPage.Handler.MauiContext.Services;
        var appState = serviceProvider.GetService<AppState>();
        var logger = serviceProvider.GetService<LoggingService>();

        // Crear y establecer el ViewModel
        _viewModel = new WebPortalViewModel(appState, logger);
        BindingContext = _viewModel;
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        // Manejar URLs especiales
        if (e.Url.StartsWith("app://"))
        {
            e.Cancel = true;
            if (e.Url.Contains("close"))
            {
                _viewModel.CloseCommand.Execute(null);
            }
        }

        // Notificar al ViewModel
        _viewModel.OnNavigating(e.Url);
    }

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        // Delegar al ViewModel pasando el WebView para autenticación
        await _viewModel.OnNavigatedAsync(e, webView);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.Dispose();
    }
}