namespace ProvidingFood2.DTO
{
	public class UpdateUserDto
	{
		
			public int UserId { get; set; } // هذا إلزامي لتحديد المستخدم
			public string? FullName { get; set; }
			public string? Email { get; set; }
			public string? PhoneNumber { get; set; }
		    public string? Password { get; set; }




	}
}
