using ProvidingFood2.DTO;

namespace ProvidingFood2.Repository
{
    public interface IAdminDonationRepository
    {
        Task<IEnumerable<DonationAdminDto>> GetAllDonationsAsync(string? region);
    }
}
