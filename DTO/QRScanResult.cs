using ProvidingFood2.Model;

namespace ProvidingFood2.DTO
{
	public class QRScanResult
	{
		
		
		public int? BondId { get; set; }
		public int? NumberOfMeals { get; set; }
		public DateTime? ExpiryDate { get; set; }

		
		public string BeneficiaryName { get; set; }
		public string RestaurantName { get; set; }
		public string Status { get; set; } 

		
		public string BeneficiaryPhone { get; set; }
	}
}
