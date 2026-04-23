using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.DTO
{
	public class DonationRequestDto

	{
		[Required]

		public int Quantity { get; set; }

		[Required]
		public DateTime DateDonated { get; set; }

		[Required]
		public string RestaurantName { get; set; }

		[Required]
		public string DeliveryLocation { get; set; }


	}
}
