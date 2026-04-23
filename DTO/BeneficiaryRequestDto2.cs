namespace ProvidingFood2.DTO
{
    public class BeneficiaryRequestDto2
    {
        public int RequestId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int FamilySize { get; set; }
        public string MaritalStatus { get; set; }

        public string MaritalStatusProofImage { get; set; }
        public string FamilySizeProofImage { get; set; }

        public string Status { get; set; }
    }
}
