using Foundation;
using UIKit;

namespace LalaHealthCare.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Forzar la solicitud de permisos de Face ID al iniciar
#if IOS
        // Esto ayuda a iOS a reconocer que necesitamos Face ID
        _ = Plugin.Fingerprint.CrossFingerprint.Current.GetAvailabilityAsync();
#endif

        return base.FinishedLaunching(application, launchOptions);
    }
}
