using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IEventDonationRepository
    {
        Task CreateAsync(EventDonation donation);
        Task UpdateStatusAsync(string sessionId, string intentId, string status);
    }
}
