using LalaHealthCare.App.Services;
using LalaHealthCare.App.ViewModels;
using LalaHealthCare.Business.Services;
using LalaHealthCare.DataAccess.Repositories;
using LalaHealthCare.DataAccess.ServiceHttp;
using LalaHealthCare.DataAccess.ServicesHttp;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Plugin.LocalNotification;
using System.Net.Http.Headers;

namespace LalaHealthCare.App;

public static class MauiProgram
{
    private static class ApiConfig
    {
        public const string BaseUrlQa = "https://apilalahealthcareqa-hdb4cqeyg3fzb4bp.eastus2-01.azurewebsites.net/";
        public const string BaseUrlDevelopment = "https://localhost:7030/";
        public const string Username = "ApiLalasUser";
        public const string Password = "zxLihO^+607=PZ";
        public const int TimeoutSeconds = 30;

        // Configurar aquí el tipo de autenticación por defecto
        public const AuthenticationType DefaultAuthType = AuthenticationType.Basic;

        // URL base según el entorno
#if DEBUG
        public const string BaseUrl = BaseUrlQa;
#else
        public const string BaseUrl = BaseUrlQa;
#endif
    }
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Agregar MudBlazor
        builder.Services.AddMudServices();

        builder.Services.AddScoped(sp =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(ApiConfig.TimeoutSeconds)
            };

            // Configurar headers por defecto
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var tokenProvider = sp.GetRequiredService<ITokenProvider>();

            return new ApiClientService(
                httpClient,                
                tokenProvider,
                ApiConfig.Username,
                ApiConfig.Password
            );
        });

        Preferences.Default.Set("numberEmergency", "911");
        Preferences.Default.Set("BaseUrlWeb", "https://lalahealthcareqa-dre0cwdkbafscmc5.eastus2-01.azurewebsites.net/");

        // Registrar servicios
        builder.Services.AddSingleton<ILoggingService, LoggingService>();
        builder.Services.AddScoped<IGeolocationService, GeolocationService>();
        builder.Services.AddSingleton<Services.INotificationService, NotificationService>();
        builder.Services.AddScoped<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IBiometricService, BiometricService>();
        builder.Services.AddScoped<ITokenProvider, TokenProvider>();
        builder.Services.AddSingleton<BiometricCredentialService>();
        builder.Services.AddSingleton(Connectivity.Current);

        // Registro de ISecureStorage (viene del paquete Microsoft.Maui.Storage)
        builder.Services.AddSingleton(SecureStorage.Default);

        // Registrar estado de la aplicación
        builder.Services.AddSingleton<AppState>();

        // Registrar repositorios
        builder.Services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
        builder.Services.AddSingleton<IVisitRepository, VisitRepository>();

        // Registrar servicios de negocio
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IVisitService, VisitService>();

        // Registrar ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<LayoutViewModel>();
        builder.Services.AddTransient<WebPortalViewModel>();

        return builder.Build();
    }
}