using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class Restaurant
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int RestaurantId { get; set; }

		[Required]
		[ForeignKey("Category")]
		public int CategoryId { get; set; }
		public Category Category { get; set; }

		[Required]
		[ForeignKey("User")]
		public int UserId { get; set; }
		public User User { get; set; }

		[MaxLength(25)]
		public string RestaurantName { get; set; }

		[MaxLength(255)]
		public string RestaurantEmail { get; set; }

		[MaxLength(255)]
		public string RestaurantPhone { get; set; }

		[MaxLength(225)]
		public string RestaurantAddress { get; set; }
	}
}
