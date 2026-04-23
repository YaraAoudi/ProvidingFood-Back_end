namespace ProvidingFood2.DTO
{
    public class CreateShelterPostDto
    {
        public int ShelterId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int RequiredMeals { get; set; }

        public IFormFile DisplayImage { get; set; }
    
}
}
