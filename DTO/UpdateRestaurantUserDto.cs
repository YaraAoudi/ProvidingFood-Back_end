using ProvidingFood2.Model;

namespace ProvidingFood2.DTO
{
	public class UpdateRestaurantUserDto
	{
		public UpdateUserDto User { get; set; }
		public UpdateRestaurantDto Restaurant { get; set; }
	}
}
