using Dapper;
using System.Data.SqlClient;

namespace ProvidingFood2.Service
{
    public class ChallengeDisplayService
    {
        private readonly string _connectionString;

        public ChallengeDisplayService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<object> GetChallengeStatus()
        {
            using var connection = new SqlConnection(_connectionString);

            var currentChallenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
        SELECT TOP 1 *
        FROM Challenges
        WHERE IsActive = 1
        AND GETUTCDATE() BETWEEN StartDate AND EndDate
        ORDER BY Id DESC");

            var lastChallenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
        SELECT TOP 1 *
        FROM Challenges
        WHERE EndDate < GETUTCDATE()
        ORDER BY EndDate DESC");

            IEnumerable<dynamic> winners = new List<dynamic>();

            if (lastChallenge != null)
            {
                winners = await connection.QueryAsync(@"
            SELECT u.FullName, uc.Points
            FROM UserChallenges uc
            JOIN [user] u ON u.UserId = uc.UserId
            WHERE uc.ChallengeId = @ChallengeId
            AND uc.Points >= 10
            ORDER BY uc.Points DESC",
                    new { ChallengeId = lastChallenge.Id });
            }

            return new
            {
                currentChallenge = currentChallenge == null ? null : new
                {
                    name = currentChallenge.Name,
                    startDate = currentChallenge.StartDate,
                    endDate = currentChallenge.EndDate
                },
                lastWinners = lastChallenge == null ? null : new
                {
                    challengeName = lastChallenge.Name,
                    winners = winners
                }
            };
        }
    }
}