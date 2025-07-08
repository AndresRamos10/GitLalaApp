using LalaHealthCare.App.Models;

namespace LalaHealthCare.App.Services;

public class GeolocationService : IGeolocationService
{
    private readonly ILoggingService _loggingService;

    public GeolocationService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<GeolocationResult> GetCurrentLocationAsync()
    {
        try
        {
            // Verificar permisos
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    await _loggingService.LogWarningAsync("Location permission denied");
                    return new GeolocationResult
                    {
                        Success = false,
                        ErrorMessage = "Se requiere permiso de ubicación para realizar el Check-In"
                    };
                }
            }

            // Obtener ubicación
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                await _loggingService.LogInformationAsync($"Location obtained: {location.Latitude}, {location.Longitude}",
                    new Dictionary<string, object>
                    {
                        { "Latitude", location.Latitude },
                        { "Longitude", location.Longitude },
                        { "Accuracy", location.Accuracy ?? 0 },
                        { "Timestamp", location.Timestamp }
                    });

                // Obtener dirección
                var address = await GetAddressFromCoordinatesAsync(location.Latitude, location.Longitude);

                return new GeolocationResult
                {
                    Success = true,
                    Latitude = decimal.Parse(location.Latitude.ToString()),
                    Longitude = decimal.Parse(location.Longitude.ToString()),
                    Address = address,
                    Accuracy = location.Accuracy
                };
            }
            else
            {
                await _loggingService.LogWarningAsync("Could not get location");
                return new GeolocationResult
                {
                    Success = false,
                    ErrorMessage = "No se pudo obtener la ubicación actual"
                };
            }
        }
        catch (FeatureNotSupportedException)
        {
            await _loggingService.LogErrorAsync("Geolocation not supported on device");
            return new GeolocationResult
            {
                Success = false,
                ErrorMessage = "La geolocalización no está soportada en este dispositivo"
            };
        }
        catch (PermissionException)
        {
            await _loggingService.LogErrorAsync("Location permission denied");
            return new GeolocationResult
            {
                Success = false,
                ErrorMessage = "Se denegó el permiso de ubicación"
            };
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Error getting location", ex);
            return new GeolocationResult
            {
                Success = false,
                ErrorMessage = $"Error al obtener la ubicación: {ex.Message}"
            };
        }
    }

    public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                var address = new List<string>();

                if (!string.IsNullOrWhiteSpace(placemark.Thoroughfare))
                    address.Add(placemark.Thoroughfare);

                if (!string.IsNullOrWhiteSpace(placemark.SubThoroughfare))
                    address.Add(placemark.SubThoroughfare);

                if (!string.IsNullOrWhiteSpace(placemark.Locality))
                    address.Add(placemark.Locality);

                if (!string.IsNullOrWhiteSpace(placemark.AdminArea))
                    address.Add(placemark.AdminArea);

                if (!string.IsNullOrWhiteSpace(placemark.PostalCode))
                    address.Add(placemark.PostalCode);

                return string.Join(", ", address);
            }

            return $"{latitude:F6}, {longitude:F6}";
        }
        catch (Exception ex)
        {
            await _loggingService.LogWarningAsync($"Error getting address from coordinates: {ex.Message}");
            return $"{latitude:F6}, {longitude:F6}";
        }
    }
}
