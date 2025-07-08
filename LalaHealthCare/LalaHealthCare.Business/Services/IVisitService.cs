using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.Business.Services;

public interface IVisitService
{
    Task<List<Visit>> GetVisitsByDateAsync(DateTime date);
    Task<List<Visit>> GetUpcomingVisitsAsync();
    Task<Visit?> GetVisitByIdAsync(string visitId);
    Task<bool> CheckInAsync(string visitId, decimal? latitude = null, decimal? longitude = null, string? address = null);
    Task<bool> CheckOutAsync(string visitId, string observations, string signatureData, decimal? latitude = null, decimal? longitude = null, string? address = null);
    Task<List<Visit>> SearchVisitsAsync(string searchTerm);
}
