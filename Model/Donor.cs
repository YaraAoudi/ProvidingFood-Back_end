using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class Donor
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int DonorId { get; set; }
		public string FoodName { get; set; }
		public string UserType{ get; set; }
		public string Description { get; set; }
		public string Image { get; set; }
		public int Quantity { get; set; }
		public DateTime DateTime { get; set; }
		public bool Vegetarian { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }
		public User User { get; set; }


		[ForeignKey("Donation")]
		public string DonationId { get; set; }
		public DonationRequestStatus Donation { get; set; }

	}
}
