using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvidingFood2.Model
{
	public class Beneficiary
	{
		public int BeneficiaryId { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public int FamilySize { get; set; }

		public string MaritalStatus { get; set; }
		public string MaritalStatusProofImage { get; set; }
		public string FamilySizeProofImage { get; set; }

		public bool IsActive { get; set; }
	}
}
