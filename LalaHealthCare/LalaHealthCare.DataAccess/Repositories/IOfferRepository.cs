using LalaHealthCare.DataAccess.Models;

namespace LalaHealthCare.DataAccess.Repositories;

public interface IOfferRepository
{
    Task<List<Offer>> GetOffersAsync(DateTime? date = null);
    Task<Offer?> GetOfferByIdAsync(int id);
    Task<bool> AcceptOfferAsync(int offerId);
    Task<bool> DeclineOfferAsync(int offerId, string reason);
    Task<int> GetPendingOffersCountAsync();
}
