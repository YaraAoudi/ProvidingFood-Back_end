using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IRestaurantRepository
	{
		Task<IEnumerable<RestaurantGetInfo>> GetRestaurantAsync();
		Task<bool> AddRestaurantUserAsync(User user, Restaurant restaurant, string userTypeName, string categoryName);
		Task<bool> UpdateRestaurantUserAsync(User newUser, Restaurant newRestaurant);
		Task<bool> DeleteRestaurantUserAsync(int userId);
	}
}
