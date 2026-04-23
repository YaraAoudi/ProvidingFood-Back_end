namespace ProvidingFood2.DTO
{
	public class UserWithType
	{
		public int UserId { get; set; }

		public string Password { get; set; }

		public string FullName { get; set; }

		public int UserTypeId { get; set; }

        public string Email { get; set; }

        public string TypeName { get; set; }
	}
}
