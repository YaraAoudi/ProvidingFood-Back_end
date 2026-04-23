using Dapper;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using System.Data.SqlClient;

namespace ProvidingFood2.Service
{
    public class ChallengeService : IChallengeService
    {
        private readonly string _connectionString;
        private readonly IChallengeRepository _repo;

        public ChallengeService(IConfiguration config, IChallengeRepository repo)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _repo = repo;
        }

       
        public async Task<bool> CanDonateToday(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            var challenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT TOP 1 *
                FROM dbo.Challenges
                WHERE IsActive = 1
                AND GETUTCDATE() BETWEEN StartDate AND EndDate
                ORDER BY Id DESC");

            if (challenge == null)
                return false; 

            var userChallenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT *
                FROM dbo.UserChallenges
                WHERE UserId = @UserId AND ChallengeId = @ChallengeId",
                new { UserId = userId, ChallengeId = challenge.Id });

            if (userChallenge == null)
                return true;

            var today = DateTime.UtcNow.Date;

            if (userChallenge.LastDonationDate != null &&
                ((DateTime)userChallenge.LastDonationDate).Date == today)
                return false; 

            return true;
        }

        public async Task AddPointAsync(int userId, string sessionId)
        {
            using var connection = new SqlConnection(_connectionString);

            var challenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT TOP 1 *
                FROM dbo.Challenges
                WHERE IsActive = 1
                AND GETUTCDATE() BETWEEN StartDate AND EndDate
                ORDER BY Id DESC");

            if (challenge == null)
            {
                Console.WriteLine("❌ No Active Challenge");
                return;
            }

            int challengeId = challenge.Id;

            var userChallenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT *
                FROM dbo.UserChallenges
                WHERE UserId = @UserId AND ChallengeId = @ChallengeId",
                new { UserId = userId, ChallengeId = challengeId });

            var today = DateTime.UtcNow.Date;

            // 🆕 أول مرة
            if (userChallenge == null)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO dbo.UserChallenges
                    (UserId, ChallengeId, Points, LastDonationDate, LastSessionId)
                    VALUES
                    (@UserId, @ChallengeId, 1, @Today, @SessionId)",
                    new
                    {
                        UserId = userId,
                        ChallengeId = challengeId,
                        Today = today,
                        SessionId = sessionId
                    });

                Console.WriteLine("✅ First time → Point added");
                return;
            }

            // ❌ نفس الدفع
            if (userChallenge.LastSessionId == sessionId)
                return;

            // ❌ نفس اليوم
            if (userChallenge.LastDonationDate != null &&
                ((DateTime)userChallenge.LastDonationDate).Date == today)
                return;

            // 🔥 streak reset إذا قطع يوم
            if (userChallenge.LastDonationDate != null &&
                ((DateTime)userChallenge.LastDonationDate).Date < today.AddDays(-1))
            {
                await connection.ExecuteAsync(@"
                    UPDATE dbo.UserChallenges
                    SET Points = 1,
                        LastDonationDate = @Today,
                        LastSessionId = @SessionId
                    WHERE UserId = @UserId AND ChallengeId = @ChallengeId",
                    new
                    {
                        Today = today,
                        SessionId = sessionId,
                        UserId = userId,
                        ChallengeId = challengeId
                    });

                Console.WriteLine("🔄 Streak reset");
                return;
            }

            // ❌ الحد الأقصى
            if ((int)userChallenge.Points >= 10)
                return;

            int newPoints = (int)userChallenge.Points + 1;

            await connection.ExecuteAsync(@"
                UPDATE dbo.UserChallenges
                SET Points = @Points,
                    LastDonationDate = @Today,
                    LastSessionId = @SessionId
                WHERE UserId = @UserId AND ChallengeId = @ChallengeId",
                new
                {
                    Points = newPoints,
                    Today = today,
                    SessionId = sessionId,
                    UserId = userId,
                    ChallengeId = challengeId
                });

            // 🏆 مكافأة
            if (newPoints == 10)
            {
                Console.WriteLine("🏆 USER COMPLETED CHALLENGE!");
                // هون فيك تضيف reward (badge / notification / gift)
            }
        }

        public async Task CreateAsync(CreateChallengeDto dto)
        {
            var challenge = new Challenge
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            };

            await _repo.CreateAsync(challenge);
        }

        public async Task EndAsync(int id)
        {
            await _repo.EndAsync(id);
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();

            return data.Select(c => new
            {
                c.Id,
                c.Name,
                c.StartDate,
                c.EndDate,
                c.IsActive
            });
        }

        public async Task<object> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);

            if (c == null) return null;

            return new
            {
                c.Id,
                c.Name,
                c.StartDate,
                c.EndDate,
                c.IsActive
            };
        }

        public async Task UpdateAsync(int id, UpdateChallengeDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);

            if (existing == null)
                throw new Exception("Challenge not found");

            var finalStartDate = dto.StartDate ?? existing.StartDate;
            var finalEndDate = dto.EndDate ?? existing.EndDate;

            if (finalEndDate <= finalStartDate)
                throw new Exception("EndDate must be greater");

            var updated = new Challenge
            {
                Name = dto.Name ?? existing.Name,
                StartDate = finalStartDate,
                EndDate = finalEndDate,
                IsActive = dto.IsActive ?? existing.IsActive
            };

            await _repo.UpdateAsync(id, updated);
        }

            public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<object>> GetLeaderboardAsync(int id)
        {
            var data = await _repo.GetLeaderboardAsync(id);

            return data.Select(x => new
            {
                fullName = x.FullName,
                points = x.Points
            });
        }

        public async Task ActivateAsync(int id, ActivateChallengeDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
                throw new Exception("EndDate must be greater than StartDate");

            if (dto.StartDate < DateTime.UtcNow.Date)
                throw new Exception("StartDate must be today or future");

            await _repo.ActivateAsync(id, dto.StartDate, dto.EndDate);
        }
        public async Task<object> GetUserPointsWithStatus(int userId)
        {
            // ✅ التحدي الحالي
            var currentChallenge = await _repo.GetCurrentChallengeAsync();

            if (currentChallenge != null)
            {
                var points = await _repo.GetUserPointsAsync(userId, currentChallenge.Id);

                return new
                {
                    points = points ?? 0,
                    challengeStatus = "current",
                    challengeName = currentChallenge.Name
                };
            }

            // ❌ آخر تحدي منتهي
            var lastChallenge = await _repo.GetLastChallengeAsync();

            if (lastChallenge == null)
            {
                return new
                {
                    points = 0,
                    challengeStatus = "none",
                    challengeName = (string)null
                };
            }

            var lastPoints = await _repo.GetUserPointsAsync(userId, lastChallenge.Id);

            return new
            {
                points = lastPoints ?? 0,
                challengeStatus = "ended",
                challengeName = lastChallenge.Name
            };
        }

        public async Task<dynamic> GetActiveChallengeAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var challenge = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
        SELECT TOP 1 *
        FROM dbo.Challenges
        WHERE IsActive = 1
        AND GETUTCDATE() BETWEEN StartDate AND EndDate
        ORDER BY Id DESC");

            return challenge;
        }
    }
}