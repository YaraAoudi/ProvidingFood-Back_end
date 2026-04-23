using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class Login
	{
		[Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
		[EmailAddress(ErrorMessage = "صيغة البريد غير صالحة")]
		public string Email { get; set; }

		[Required(ErrorMessage = "كلمة المرور مطلوبة")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

	
	}
}
