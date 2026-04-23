namespace ProvidingFood2.DTO
{
	public class DonorWithStatusDto
	{
		public int FoodId { get; set; }
		public int RequesId { get; set; }
		public string FoodName { get; set; }
		public string Description { get; set; }
		public string Country { get; set; }
		public bool Vegetarian { get; set; }
		public string Status { get; set; }
	}
}
