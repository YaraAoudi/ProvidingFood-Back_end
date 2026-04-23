namespace ProvidingFood2.DTO
{
	public class FoodBondResponse
	{

		public int Id { get; set; }
		public string BeneficiaryName { get; set; } = string.Empty;
		public string RestaurantName { get; set; } = string.Empty;
		public int NumberOfMeals { get; set; }
		public string QRCode { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiryDate { get; set; }
		public string StatusName { get; set; }
	}
}
