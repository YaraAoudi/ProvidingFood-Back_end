using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class UserType
	{
		[Key]
		public int UserTypeId { get; set; }

		[MaxLength(255)]
		public string TypeName { get; set; }

		public ICollection<User> Users { get; set; }
	}
}
