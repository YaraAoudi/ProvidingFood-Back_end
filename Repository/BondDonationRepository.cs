using Dapper;
using ProvidingFood2.Model;        
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class BondDonationRepository : IBondDonationRepository
    {
        private readonly string _connectionString;

        public BondDonationRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task CreateAsync(BondDonation d)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
        INSERT INTO BondDonations
        (DonorUserId, NumberOfBonds, BondPrice, TotalAmount,
         StripeSessionId, Status, CreatedAt)
        VALUES
        (@DonorUserId, @NumberOfBonds, @BondPrice, @TotalAmount,
         @StripeSessionId, @Status, @CreatedAt)", d);
        }

        public async Task<int> UpdateStatusAsync(string sessionId, string paymentIntentId, string status)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(@"
    UPDATE BondDonations
    SET Status = @Status,
        PaymentIntentId = @PaymentIntentId
    WHERE StripeSessionId = @SessionId",
            new
            {
                SessionId = sessionId,
                PaymentIntentId = paymentIntentId,
                Status = status
            });

            return rows;
        }

        public async Task<IEnumerable<BondDonation>> GetUnassignedAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<BondDonation>(@"
        SELECT * FROM BondDonations
        WHERE Assigned = 0 AND Status = 'Paid'");
        }

        public async Task<DateTime?> GetLastDonationDateAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<DateTime?>(@"
    SELECT TOP 1 CreatedAt
    FROM BondDonations
    WHERE DonorUserId = @UserId
    AND CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
    ORDER BY CreatedAt DESC",
     new { UserId = userId });
        }


        }
    }
