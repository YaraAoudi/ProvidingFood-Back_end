namespace ProvidingFood2.Model
{
	public class LoginResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string Token { get; set; }
		public int UserId { get; set; }
		public string FullName { get; set; }
		public int UserTypeId { get; set; }

	}
}
