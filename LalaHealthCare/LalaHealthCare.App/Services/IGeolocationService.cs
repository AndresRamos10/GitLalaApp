using LalaHealthCare.App.Models;

namespace LalaHealthCare.App.Services;

public interface IGeolocationService
{
    Task<GeolocationResult> GetCurrentLocationAsync();
    Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude);
}