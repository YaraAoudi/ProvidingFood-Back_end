using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IGiftDonationRepository
    {
        Task CreateAsync(GiftDonation donation);
        Task<int> UpdateStatusGiftAsync(string sessionId, string paymentIntentId, string status);
    }
}
