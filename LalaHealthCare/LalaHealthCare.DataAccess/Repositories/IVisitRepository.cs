using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.DataAccess.Repositories;

public interface IVisitRepository
{
    Task<List<Visit>> GetVisitsByNurseAndDateAsync(string nurseId, DateTime date);
    Task<Visit?> GetVisitByIdAsync(string visitId);
    Task<bool> UpdateVisitAsync(Visit visit);
    Task<bool> CheckInVisitAsync(CheckInDto data);
    Task<bool> CheckOutVisitAsync(CheckOutDto data);
}