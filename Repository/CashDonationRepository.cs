using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class CashDonationRepository : ICashDonationRepository
    {
        private readonly string _connectionString;

        public CashDonationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ إنشاء التبرع
        public async Task<int> CreateAsync(CashDonation donation)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(@"
INSERT INTO CashDonations
(DonorUserId, Amount, RegionId, RegionName, StripeSessionId, PaymentIntentId, Status, CreatedAt)
VALUES
(@DonorUserId, @Amount, @RegionId, @RegionName, @StripeSessionId, @PaymentIntentId, @Status, @CreatedAt);

SELECT CAST(SCOPE_IDENTITY() as int);", donation);
        }

        // ✅ تحديث الحالة (الأهم)
        public async Task<int> UpdateStatusAsync(string sessionId, string paymentIntentId, string status)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(@"
UPDATE CashDonations
SET 
    Status = @Status,
    PaymentIntentId = COALESCE(PaymentIntentId, @PaymentIntentId)
WHERE 
    StripeSessionId = @SessionId",
            new
            {
                SessionId = sessionId,
                PaymentIntentId = paymentIntentId,
                Status = status
            });

            return rows;
        }
    }
}