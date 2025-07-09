using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.DataAccess.Repositories;

public class MockOfferRepository : IOfferRepository
{
    private readonly List<Offer> _offers;

    public MockOfferRepository()
    {
        var today = DateTime.Today;
        var baseTime = new DateTime(today.Year, today.Month, today.Day, 6, 30, 0);

        _offers = new List<Offer>
            {
                new Offer
                {
                    Id = 1,
                    PatientId = 101,
                    PatientName = "Carmela Gonzales",
                    PatientImageUrl = "https://i.pravatar.cc/150?img=1",
                    ScheduledDateTime = baseTime,
                    Duration = TimeSpan.FromMinutes(45),
                    ServiceCode = "HOMECARE",
                    ServiceType = "Single Visit (fr)",
                    LocationAddress = "Montreal, H2W2R2",
                    LocationPostalCode = "H2W2R2",
                    Latitude = 45.5017,
                    Longitude = -73.5673,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-2),
                    IsViewed = true
                },
                new Offer
                {
                    Id = 2,
                    PatientId = 102,
                    PatientName = "Andres Ramos",
                    PatientImageUrl = "https://i.pravatar.cc/150?img=2",
                    ScheduledDateTime = baseTime.AddHours(2),
                    Duration = TimeSpan.FromMinutes(60),
                    ServiceCode = "HOMECARE",
                    ServiceType = "Single Visit (fr)",
                    LocationAddress = "Montreal, H3A1B2",
                    LocationPostalCode = "H3A1B2",
                    Latitude = 45.5048,
                    Longitude = -73.5772,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1.5),
                    IsViewed = false
                },
                new Offer
                {
                    Id = 3,
                    PatientId = 103,
                    PatientName = "Camila Restrepo",
                    PatientImageUrl = "https://i.pravatar.cc/150?img=3",
                    ScheduledDateTime = baseTime.AddHours(4),
                    Duration = TimeSpan.FromMinutes(30),
                    ServiceCode = "HOMECARE",
                    ServiceType = "Single Visit (fr)",
                    LocationAddress = "Montreal, H4B1R5",
                    LocationPostalCode = "H4B1R5",
                    Latitude = 45.4938,
                    Longitude = -73.6538,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1),
                    IsViewed = false
                },
                new Offer
                {
                    Id = 4,
                    PatientId = 104,
                    PatientName = "Saul Hernandez",
                    PatientImageUrl = "https://i.pravatar.cc/150?img=4",
                    ScheduledDateTime = baseTime.AddHours(6),
                    Duration = TimeSpan.FromMinutes(45),
                    ServiceCode = "HOMECARE",
                    ServiceType = "Single Visit (fr)",
                    LocationAddress = "Montreal, H1Y2K8",
                    LocationPostalCode = "H1Y2K8",
                    Latitude = 45.5428,
                    Longitude = -73.5984,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.Now.AddMinutes(-30),
                    IsViewed = false
                }
            };
    }

    public Task<List<Offer>> GetOffersAsync(DateTime? date = null)
    {
        var query = _offers.AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(o => o.ScheduledDateTime.Date == date.Value.Date);
        }

        return Task.FromResult(query.OrderBy(o => o.ScheduledDateTime).ToList());
    }

    public Task<Offer?> GetOfferByIdAsync(int id)
    {
        var offer = _offers.FirstOrDefault(o => o.Id == id);
        if (offer != null && !offer.IsViewed)
        {
            offer.IsViewed = true;
        }
        return Task.FromResult(offer);
    }

    public Task<bool> AcceptOfferAsync(int offerId)
    {
        var offer = _offers.FirstOrDefault(o => o.Id == offerId);
        if (offer != null && offer.Status == OfferStatus.Pending)
        {
            offer.Status = OfferStatus.Accepted;
            offer.RespondedAt = DateTime.Now;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DeclineOfferAsync(int offerId, string reason)
    {
        var offer = _offers.FirstOrDefault(o => o.Id == offerId);
        if (offer != null && offer.Status == OfferStatus.Pending)
        {
            offer.Status = OfferStatus.Declined;
            offer.DeclineReason = reason;
            offer.RespondedAt = DateTime.Now;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<int> GetPendingOffersCountAsync()
    {
        return Task.FromResult(_offers.Count(o => o.Status == OfferStatus.Pending));
    }
}
