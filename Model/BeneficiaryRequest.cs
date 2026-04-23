namespace ProvidingFood2.Model
{
	public class BeneficiaryRequest
	{
		public int RequestId { get; set; }
		public int UserId { get; set; }

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int FamilySize { get; set; }

        public string MaritalStatus { get; set; }

        public string MaritalStatusProofImage { get; set; }
        public string FamilySizeProofImage { get; set; }
        public RequestStatus Status { get; set; }
    }
}
