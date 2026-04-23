using ProvidingFood2.DTO;

namespace ProvidingFood2.Service
{
    public interface IChallengeService
    {
        Task AddPointAsync(int userId, string sessionId);
        Task<bool> CanDonateToday(int userId);
        Task CreateAsync(CreateChallengeDto dto);
        Task EndAsync(int id);
        Task<IEnumerable<object>> GetAllAsync();
        Task<object> GetByIdAsync(int id);
        Task UpdateAsync(int id, UpdateChallengeDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<object>> GetLeaderboardAsync(int id);
        Task ActivateAsync(int id, ActivateChallengeDto dto);
        Task<object> GetUserPointsWithStatus(int userId);
        Task<dynamic> GetActiveChallengeAsync();
    }
}
