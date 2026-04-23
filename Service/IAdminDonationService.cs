using ProvidingFood2.DTO;

namespace ProvidingFood2.Service
{
    public interface IAdminDonationService
    {
        Task<IEnumerable<DonationAdminDto>> GetAllDonationsAsync(string? region);
    }
}
