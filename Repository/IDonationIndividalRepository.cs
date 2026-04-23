using ProvidingFood2.DTO;

namespace ProvidingFood2.Repository
{
	public interface IDonationIndividalRepository
	{
		Task<bool> AddDonationAsync(DonorDto donorDto);
		Task<IEnumerable<DonorWithStatusDto>> GetDonationsByUserEmailAsync(string userEmail);
		Task<bool> UpdateDonationStatusAsync(int requesId, string newStatus);
		Task<IEnumerable<DonorWithStatusDto>> GetAllDonationsAsync();


	}
}
