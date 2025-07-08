namespace LalaHealthCare.App.Models;

public class BiometricSettings
{
    public bool IsEnabled { get; set; }
    public bool AllowFallbackToPassword { get; set; } = true;
    public int MaxFailedAttempts { get; set; } = 3;
    public DateTime LastSuccessfulAuth { get; set; }
    public string PreferredMethod { get; set; } = "Any";
}
