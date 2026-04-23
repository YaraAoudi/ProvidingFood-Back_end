using ProvidingFood2.DTO;

namespace ProvidingFood2.Repository
{
	public interface IDonationRestaurantRepository
	{
		Task<IEnumerable<DonationRequestDto>> GetDonationsAsync();
		Task<bool> AddDonationAsync(string restaurantName, int quantity, DateTime dateDonated, string deliveryLocation);
	}
}
