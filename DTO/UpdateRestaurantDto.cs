namespace ProvidingFood2.DTO
{
	public class UpdateRestaurantDto
	{
		public int UserId { get; set; } // مفتاح الربط
		public string? RestaurantName { get; set; }
		public string? RestaurantEmail { get; set; }
		public string? RestaurantPhone { get; set; }
		public string? RestaurantAddress { get; set; }
		

	}
}
