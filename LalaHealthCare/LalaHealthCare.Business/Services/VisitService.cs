using LalaHealthCare.DataAccess.Models;
using LalaHealthCare.DataAccess.Repositories;

namespace LalaHealthCare.Business.Services;

public class VisitService : IVisitService
{
    private readonly IVisitRepository _visitRepository;
    private readonly IAuthenticationService _authService;

    public VisitService(IVisitRepository visitRepository, IAuthenticationService authService)
    {
        _visitRepository = visitRepository;
        _authService = authService;
    }

    public async Task<List<Visit>> GetVisitsByDateAsync(DateTime date)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return new List<Visit>();
        }

        return await _visitRepository.GetVisitsByNurseAndDateAsync(currentUser.ClinicianId??"0", date);
    }

    public async Task<List<Visit>> GetUpcomingVisitsAsync()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return new List<Visit>();
        }

        var todayVisits = await _visitRepository.GetVisitsByNurseAndDateAsync(currentUser.ClinicianId, DateTime.Today);
        var tomorrowVisits = await _visitRepository.GetVisitsByNurseAndDateAsync(currentUser.ClinicianId, DateTime.Today.AddDays(1));

        return todayVisits.Concat(tomorrowVisits)
            .Where(v => v.Status != VisitStatus.Completed && v.Status != VisitStatus.Cancelled)
            .OrderBy(v => v.ScheduledDateTime)
            .ToList();
    }

    public async Task<Visit?> GetVisitByIdAsync(string visitId)
    {
        return await _visitRepository.GetVisitByIdAsync(visitId);
    }

    public async Task<bool> CheckInAsync(string visitId, decimal? latitude = null, decimal? longitude = null, string? address = null)
    {
        var data = new CheckInDto
        {
            ScheduleId = visitId,
            CheckInTime = DateTime.Now,
            CheckInLatitude = latitude??0,
            CheckInLongitude = longitude??0,
            CheckInAddress = address
        };

        return await _visitRepository.CheckInVisitAsync(data);
    }

    public async Task<bool> CheckOutAsync(string visitId, string observations, string signatureData, decimal? latitude = null, decimal? longitude = null, string? address = null)
    {
       var data = new CheckOutDto
        {
            ScheduleId = visitId,
            CheckOutTime = DateTime.Now,
            Observations = observations,
            SignatureData = signatureData,
            CheckOutLatitude = latitude ?? 0,
            CheckOutLongitude = longitude ?? 0,
            CheckOutAddress = address
        };

        return await _visitRepository.CheckOutVisitAsync(data);
    }

    public async Task<List<Visit>> SearchVisitsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetVisitsByDateAsync(DateTime.Today);
        }

        var allVisits = await GetVisitsByDateAsync(DateTime.Today);

        return allVisits.Where(v =>
            v.PatientName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            v.Location.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }
}