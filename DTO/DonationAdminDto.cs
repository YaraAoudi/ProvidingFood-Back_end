namespace ProvidingFood2.DTO
{
    public class DonationAdminDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public decimal Amount { get; set; }
        public string RegionName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
