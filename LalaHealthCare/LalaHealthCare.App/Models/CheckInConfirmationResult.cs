namespace LalaHealthCare.App.Models;

public class CheckInConfirmationResult
{
    public bool Confirmed { get; set; }
    public GeolocationResult? LocationResult { get; set; }
    public DateTime CheckInTime { get; set; }
}
