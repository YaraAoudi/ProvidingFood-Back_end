using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IBondDonationRepository
    {
        Task CreateAsync(BondDonation d);
        Task<int> UpdateStatusAsync(string sessionId, string paymentIntentId, string status);
        Task<IEnumerable<BondDonation>> GetUnassignedAsync();
        Task<DateTime?> GetLastDonationDateAsync(int userId);

    }
}
