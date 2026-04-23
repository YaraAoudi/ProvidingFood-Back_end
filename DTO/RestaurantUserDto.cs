namespace ProvidingFood2.DTO
{
	public class RestaurantUserDto
	{
		// User fields
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string PhoneNumber { get; set; }
		public string UserTypeName{ get; set; }

		// Restaurant fields
		public string RestaurantName { get; set; }
		public string RestaurantEmail { get; set; }
		public string RestaurantPhone { get; set; }
		public string RestaurantAddress { get; set; }
		public string CategoryName { get; set; }
	}
}
