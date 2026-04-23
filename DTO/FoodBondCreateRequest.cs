using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.DTO
{
	public class FoodBondCreateRequest
	{
		[Required]
		public string BeneficiaryName { get; set; }  

		[Required]
		public string RestaurantName { get; set; }  


		[Required]
		[Range(1, 100)]
		public int NumberOfMeals { get; set; }

		[Required]
		public DateTime ExpiryDate { get; set; }

	}
}
