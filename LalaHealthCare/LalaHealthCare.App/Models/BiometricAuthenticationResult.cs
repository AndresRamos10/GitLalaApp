namespace LalaHealthCare.App.Models;

public class BiometricAuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public BiometricAuthenticationStatus Status { get; set; }
}

public enum BiometricAuthenticationStatus
{
    Succeeded,
    Failed,
    Cancelled,
    NotAvailable,
    NotEnrolled,
    PermissionDenied,
    Unknown
}

public enum BiometricAuthenticationType
{
    None,
    Fingerprint,
    Face,
    Iris,
    Unknown
}
