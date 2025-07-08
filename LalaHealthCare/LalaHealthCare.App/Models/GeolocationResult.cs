namespace LalaHealthCare.App.Models;

public class GeolocationResult
{
    public bool Success { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Address { get; set; }
    public string? ErrorMessage { get; set; }
    public double? Accuracy { get; set; }
}
