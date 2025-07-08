using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.DataAccess.Repositories;

public class MockVisitRepository
{
    private readonly List<Visit> _mockVisits;

    public MockVisitRepository()
    {
        var today = DateTime.Today;
        _mockVisits = new List<Visit>
        {
            new Visit
            {
                Id = "v1",
                PatientId = 1,
                PatientName = "Carmela Gonzales",
                PatientProfilePictureUrl = "https://i.pravatar.cc/150?img=5",
                ScheduledDateTime = today.AddHours(11).AddMinutes(30),
                Location = "Montreal Q11222",
                Status = VisitStatus.Completed,
                NurseId = "1",
                CheckInTime = today.AddHours(11).AddMinutes(25),
                CheckOutTime = today.AddHours(11).AddMinutes(50)
            },
            new Visit
            {
                Id = "v2",
                PatientId = 2,
                PatientName = "Andres Ramos",
                PatientProfilePictureUrl = "https://i.pravatar.cc/150?img=8",
                ScheduledDateTime = today.AddHours(11).AddMinutes(30),
                Location = "Montreal Q11222",
                Status = VisitStatus.Planned,
                NurseId = "1"
            },
            new Visit
            {
                Id = "v3",
                PatientId = 3,
                PatientName = "Camila Restrepo",
                PatientProfilePictureUrl = "https://i.pravatar.cc/150?img=9",
                ScheduledDateTime = today.AddHours(11).AddMinutes(30),
                Location = "Montreal Q11222",
                Status = VisitStatus.Planned,
                NurseId = "1"
            },
            new Visit
            {
                Id = "v4",
                PatientId = 4,
                PatientName = "Saul Hernandez",
                PatientProfilePictureUrl = "https://i.pravatar.cc/150?img=11",
                ScheduledDateTime = today.AddHours(13).AddMinutes(30),
                Location = "NY City",
                Status = VisitStatus.InProgress,
                NurseId = "1",
                CheckInTime = today.AddHours(13).AddMinutes(15)
            },
            // Agregar algunas visitas para otros días
            new Visit
            {
                Id = "v5",
                PatientId = 5,
                PatientName = "Maria Rodriguez",
                PatientProfilePictureUrl = "https://i.pravatar.cc/150?img=20",
                ScheduledDateTime = today.AddDays(1).AddHours(9),
                Location = "Brooklyn",
                Status = VisitStatus.Planned,
                NurseId = "1"
            }
        };
    }

    public async Task<List<Visit>> GetVisitsByNurseAndDateAsync(string nurseId, DateTime date)
    {
        await Task.Delay(300); // Simular latencia

        return _mockVisits
            .Where(v => v.NurseId == nurseId && v.ScheduledDateTime.Date == date.Date)
            .OrderBy(v => v.ScheduledDateTime)
            .ToList();
    }

    public async Task<Visit?> GetVisitByIdAsync(string visitId)
    {
        await Task.Delay(100);
        return _mockVisits.FirstOrDefault(v => v.Id == visitId);
    }

    public async Task<bool> UpdateVisitAsync(Visit visit)
    {
        await Task.Delay(200);

        var existingVisit = _mockVisits.FirstOrDefault(v => v.Id == visit.Id);
        if (existingVisit != null)
        {
            existingVisit.Status = visit.Status;
            existingVisit.CheckInTime = visit.CheckInTime;
            existingVisit.CheckOutTime = visit.CheckOutTime;
            existingVisit.Notes = visit.Notes;

            // Actualizar datos de geolocalización
            existingVisit.CheckInLatitude = visit.CheckInLatitude;
            existingVisit.CheckInLongitude = visit.CheckInLongitude;
            existingVisit.CheckInAddress = visit.CheckInAddress;
            existingVisit.CheckOutLatitude = visit.CheckOutLatitude;
            existingVisit.CheckOutLongitude = visit.CheckOutLongitude;
            existingVisit.CheckOutAddress = visit.CheckOutAddress;

            // Actualizar observaciones y firma
            existingVisit.Observations = visit.Observations;
            existingVisit.SignatureData = visit.SignatureData;

            return true;
        }

        return false;
    }
}