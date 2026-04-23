using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.DTO
{
	public class LoginDto
	{
		[Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
		[EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
		public string Email { get; set; }

		[Required(ErrorMessage = "كلمة المرور مطلوبة")]
		public string Password { get; set; }
	}
}
