namespace ProvidingFood2.Model
{
	public class FoodBond
	{
		public int BondId { get; set; }
		public int BeneficiaryId { get; set; }
		public int RestaurantId { get; set; }
		public int StatusId { get; set; }
		public string QRCode { get; set; }
		public int NumberOfMeals { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiryDate { get; set; }
	}
}
