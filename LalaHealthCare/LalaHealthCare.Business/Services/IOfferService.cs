using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.Business.Services;

public interface IOfferService
{
    Task<List<Offer>> GetOffersForDateAsync(DateTime date);
    Task<Offer?> GetOfferDetailsAsync(int offerId);
    Task<bool> AcceptOfferAsync(int offerId);
    Task<bool> DeclineOfferAsync(int offerId, string reason);
    Task<int> GetPendingOffersCountAsync();
}
