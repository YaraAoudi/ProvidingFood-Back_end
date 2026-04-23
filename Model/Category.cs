using System.ComponentModel.DataAnnotations;

namespace ProvidingFood2.Model
{
	public class Category
	{
		[Key]
		public int CategoryId { get; set; }

		[MaxLength(255)]
		public string Name { get; set; }
	}
}
