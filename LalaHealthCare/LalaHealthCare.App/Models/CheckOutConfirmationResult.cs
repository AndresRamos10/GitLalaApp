namespace LalaHealthCare.App.Models;

public class CheckOutConfirmationResult
{
    public bool Confirmed { get; set; }
    public string Observations { get; set; } = string.Empty;
    public string SignatureData { get; set; } = string.Empty;
    public GeolocationResult? LocationResult { get; set; }
}
