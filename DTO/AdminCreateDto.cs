using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.DTO
{
	public class AdminCreateDto
	{
		[Required]
		public string FullName { get; set; }

		
		[Required, EmailAddress]
		public string Email { get; set; }

		[Required]
		public string PhoneNumber { get; set; }

		[Required]
		public string Password { get; set; }

		[Required]
		public string UserTypeName { get; set; }

		[Required]
		public string Position { get; set; }
	}
}
