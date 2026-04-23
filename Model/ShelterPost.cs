namespace ProvidingFood2.Model
{
    public class ShelterPost
    {
        public int Id { get; set; }
        public int ShelterId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public int RequiredMeals { get; set; }
        public int CollectedMeals { get; set; }

        public string DisplayImageUrl { get; set; }

        public string Status { get; set; } // Active / Completed

        public DateTime CreatedAt { get; set; }
    }
}
