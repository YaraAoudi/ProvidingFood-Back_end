using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class DonationRequestStatus
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int DonationId { get; set; }
		public int DonationName { get; set; }
	}
}
