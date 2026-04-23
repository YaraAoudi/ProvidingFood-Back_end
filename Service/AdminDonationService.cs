using ProvidingFood2.DTO;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Service
{
    public class AdminDonationService : IAdminDonationService
    {
        private readonly IAdminDonationRepository _repo;

        public AdminDonationService(IAdminDonationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<DonationAdminDto>> GetAllDonationsAsync(string? region)
        {
            return await _repo.GetAllDonationsAsync(region);
        }
    }
}
