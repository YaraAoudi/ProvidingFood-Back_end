using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface ICashDonationRepository
    {
        Task<int> CreateAsync(CashDonation donation);
        Task<int> UpdateStatusAsync(string sessionId, string paymentIntentId, string status);
    }
}
