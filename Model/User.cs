using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class User
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserId { get; set; }

		[Required]
		[ForeignKey("UserType")]
		public int UserTypeId { get; set; }
		public UserType UserType { get; set; }

		[MaxLength(25)]
		public string FullName { get; set; }

		[MaxLength(255)]
		public string Email { get; set; }

		[MaxLength(255)]
		public string Password { get; set; }

		[MaxLength(255)]
		public string PhoneNumber { get; set; }

		public DateTime? Birthday { get; set; }
		public int? DailyNeed { get; set; }

	}
}
