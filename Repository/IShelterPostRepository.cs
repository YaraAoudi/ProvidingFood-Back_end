using ProvidingFood2.DTO;
using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IShelterPostRepository
    {

        Task<int> CreateAsync(ShelterPost post);
        Task<IEnumerable<ShelterPost>> GetAllAsync();
        Task<ShelterPost> GetByIdAsync(int id);
        Task UpdateAsync(ShelterPost post);
        Task<Shelter> GetShelterByUserId(int userId);
        Task<IEnumerable<ShelterPostWithDonationsDto>> GetPostsWithDonations();
        Task AddDonationAsync(int postId, int restaurantId, int meals);
        Task<int?> GetRestaurantIdByUserId(int userId);
        Task<int> GetShelterOwnerUserIdByPostId(int postId);
        Task<string> GetRestaurantNameById(int restaurantId);
    }
}
