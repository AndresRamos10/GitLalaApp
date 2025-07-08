using LalaHealthCare.DataAccess.Models;
using LalaHealthCare.DataAccess.ServicesHttp;

namespace LalaHealthCare.DataAccess.Repositories;

public class VisitRepository : IVisitRepository
{
    private readonly ApiClientService _apiClient;
    private readonly IConnectivity _connectivity;

    public VisitRepository(ApiClientService apiClient, IConnectivity connectivity)
    {
        _apiClient = apiClient;        
        _connectivity = connectivity;
    }

    public async Task<List<Visit>> GetVisitsByNurseAndDateAsync(string nurseId, DateTime date)
    {
        // Verificar conectividad
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("No internet connection available");
        }

        // Formatear fecha para la API
        var dateStr = date.ToString("yyyy-MM-dd");
        var endpoint = $"api/Note/GetNotesNurse?nurseId={nurseId}&date={dateStr}";

        var visits = await _apiClient.GetAsync<List<Visit>>(endpoint);

        // Ordenar por hora programada como hace el mock
        return visits?.OrderBy(v => v.ScheduledDateTime).ToList() ?? new List<Visit>();
    }

    public async Task<Visit?> GetVisitByIdAsync(string visitId)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("no internet connection available");
        }

        var endpoint = $"api/visits/{visitId}";
        return await _apiClient.GetAsync<Visit>(endpoint);
    }

    public async Task<bool> CheckInVisitAsync(CheckInDto data) {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("no internet connection available");
        }

        var endpoint = $"api/Note/CheckInTimeSheet";
        return await _apiClient.PostAsync<CheckInDto, bool>(endpoint, data);
    }
    public async Task<bool> CheckOutVisitAsync(CheckOutDto data) {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("no internet connection available");
        }

        var endpoint = $"api/Note/CheckOutTimeSheet";
        return await _apiClient.PostAsync<CheckOutDto, bool>(endpoint, data);
    }

    public async Task<bool> UpdateVisitAsync(Visit visit)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("No internet connection available");
        }

        var endpoint = $"/api/visits/{visit.Id}";

        // Crear DTO con solo los campos necesarios para actualizar
        var updateRequest = new VisitUpdateRequest
        {
            Status = visit.Status.ToString(),
            CheckInTime = visit.CheckInTime,
            CheckOutTime = visit.CheckOutTime,
            Notes = visit.Notes,
            CheckInLatitude = visit.CheckInLatitude,
            CheckInLongitude = visit.CheckInLongitude,
            CheckInAddress = visit.CheckInAddress,
            CheckOutLatitude = visit.CheckOutLatitude,
            CheckOutLongitude = visit.CheckOutLongitude,
            CheckOutAddress = visit.CheckOutAddress,
            Observations = visit.Observations,
            SignatureData = visit.SignatureData
        };

        var response = await _apiClient.PutAsync<VisitUpdateRequest, VisitUpdateResponse>(endpoint, updateRequest);

        return response?.Success ?? false;
    }

    // DTOs internos para las peticiones
    private class VisitUpdateRequest
    {
        public string Status { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Notes { get; set; }
        public decimal? CheckInLatitude { get; set; }
        public decimal? CheckInLongitude { get; set; }
        public string? CheckInAddress { get; set; }
        public decimal? CheckOutLatitude { get; set; }
        public decimal? CheckOutLongitude { get; set; }
        public string? CheckOutAddress { get; set; }
        public string? Observations { get; set; }
        public string? SignatureData { get; set; }
    }

    private class VisitUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Visit UpdatedVisit { get; set; }
    }
}