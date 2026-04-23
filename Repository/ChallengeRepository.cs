using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly string _connectionString;

        public ChallengeRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task CreateAsync(Challenge challenge)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
                INSERT INTO Challenges (Name, StartDate, EndDate, IsActive)
                VALUES (@Name, @StartDate, @EndDate, 1)", challenge);
        }

        public async Task EndAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
                UPDATE Challenges
                SET IsActive = 0,
                    EndDate = GETUTCDATE()
                WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Challenge>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Challenge>("SELECT * FROM Challenges ORDER BY Id DESC");
        }

        public async Task<Challenge> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Challenge>(
                "SELECT * FROM Challenges WHERE Id = @Id",
                new { Id = id });
        }

        public async Task UpdateAsync(int id, Challenge challenge)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
        UPDATE Challenges
        SET Name = @Name,
            StartDate = @StartDate,
            EndDate = @EndDate,
            IsActive = @IsActive
        WHERE Id = @Id",
                new
                {
                    Id = id,
                    challenge.Name,
                    challenge.StartDate,
                    challenge.EndDate,
                    challenge.IsActive
                });
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM Challenges WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<IEnumerable<dynamic>> GetLeaderboardAsync(int challengeId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync(@"
                SELECT u.FullName, uc.Points
                FROM UserChallenges uc
                JOIN [user] u ON u.UserId = uc.UserId
                WHERE uc.ChallengeId = @ChallengeId
                ORDER BY uc.Points DESC",
                new { ChallengeId = challengeId });
        }
        public async Task ActivateAsync(int id, DateTime startDate, DateTime endDate)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"UPDATE Challenges SET IsActive = 0");

            await connection.ExecuteAsync(@"
        UPDATE Challenges
        SET IsActive = 1,
            StartDate = @StartDate,
            EndDate = @EndDate
        WHERE Id = @Id",
                new
                {
                    Id = id,
                    StartDate = startDate,
                    EndDate = endDate
                });
        }
        public async Task<dynamic> GetCurrentChallengeAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT TOP 1 *
            FROM Challenges
            WHERE IsActive = 1
            AND GETUTCDATE() BETWEEN StartDate AND EndDate
            ORDER BY Id DESC");
        }

        public async Task<dynamic> GetLastChallengeAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT TOP 1 *
            FROM Challenges
            WHERE EndDate < GETUTCDATE()
            ORDER BY EndDate DESC");
        }

        public async Task<int?> GetUserPointsAsync(int userId, int challengeId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<int?>(@"
            SELECT Points
            FROM UserChallenges
            WHERE UserId = @UserId AND ChallengeId = @ChallengeId",
                new { UserId = userId, ChallengeId = challengeId });
        }

    }
}