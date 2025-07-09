using LalaHealthCare.DataAccess.Models;
using LalaHealthCare.DataAccess.Repositories;

namespace LalaHealthCare.Business.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;

    public OfferService(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<List<Offer>> GetOffersForDateAsync(DateTime date)
    {
        return await _offerRepository.GetOffersAsync(date);
    }

    public async Task<Offer?> GetOfferDetailsAsync(int offerId)
    {
        return await _offerRepository.GetOfferByIdAsync(offerId);
    }

    public async Task<bool> AcceptOfferAsync(int offerId)
    {
        // Aquí podrías agregar lógica adicional como:
        // - Verificar conflictos de horario
        // - Notificar al sistema central
        // - Crear la visita correspondiente
        return await _offerRepository.AcceptOfferAsync(offerId);
    }

    public async Task<bool> DeclineOfferAsync(int offerId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Decline reason is required", nameof(reason));
        }

        return await _offerRepository.DeclineOfferAsync(offerId, reason);
    }

    public async Task<int> GetPendingOffersCountAsync()
    {
        return await _offerRepository.GetPendingOffersCountAsync();
    }
}
