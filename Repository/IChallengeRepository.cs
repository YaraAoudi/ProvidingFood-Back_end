using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IChallengeRepository
    {
        Task CreateAsync(Challenge challenge);
        Task EndAsync(int id);
        Task<IEnumerable<Challenge>> GetAllAsync();
        Task<Challenge> GetByIdAsync(int id);
        Task UpdateAsync(int id, Challenge challenge);
        Task DeleteAsync(int id);
        Task<IEnumerable<dynamic>> GetLeaderboardAsync(int challengeId);
        Task ActivateAsync(int id, DateTime startDate, DateTime endDate);
        Task<dynamic> GetCurrentChallengeAsync();
        Task<dynamic> GetLastChallengeAsync();
        Task<int?> GetUserPointsAsync(int userId, int challengeId);
    }
}