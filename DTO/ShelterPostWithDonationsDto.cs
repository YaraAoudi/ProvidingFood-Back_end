namespace ProvidingFood2.DTO
{
    public class ShelterPostWithDonationsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShelterName { get; set; }
        public string Description { get; set; }

        public int RequiredMeals { get; set; }
        public int CollectedMeals { get; set; }

        public string Status { get; set; }
        public string DisplayImageUrl { get; set; }

        public List<DonationDto> Donations { get; set; }
    }
}
