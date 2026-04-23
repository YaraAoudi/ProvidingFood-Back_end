namespace ProvidingFood2.DTO
{
    public class BeneficiaryRequestDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int FamilySize { get; set; }

        public string MaritalStatus { get; set; }

        public IFormFile MaritalStatusProofImage { get; set; }
        public IFormFile FamilySizeProofImage { get; set; }
    }
}
