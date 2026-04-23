using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class BondRepository : IBondRepository
    {
        private readonly string _connectionString;

        public BondRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<decimal> GetActiveBondPriceAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var price = await connection.ExecuteScalarAsync<decimal?>(@"
            SELECT TOP 1 Price 
            FROM BondSettings 
            WHERE IsActive = 1
            ORDER BY CreatedAt DESC");

            return price ?? 0;
        }
        public async Task SetNewBondPriceAsync(decimal newPrice)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(); // 🔥 هذا هو الحل

            using var transaction = connection.BeginTransaction();

            try
            {
                // ❌ إلغاء القديم
                await connection.ExecuteAsync(@"
UPDATE BondSettings
SET IsActive = 0
WHERE IsActive = 1",
                transaction: transaction);

                // ✅ إضافة الجديد
                await connection.ExecuteAsync(@"
INSERT INTO BondSettings (Price, IsActive, CreatedAt)
VALUES (@Price, 1, GETUTCDATE())",
                new { Price = newPrice },
                transaction: transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<IEnumerable<GiftDonation>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var result = await connection.QueryAsync<GiftDonation>(@"
SELECT 
    GiftDonationId,
    DonorUserId,
    RecipientName,
    RecipientPhone,
    RecipientAddress,
    NumberOfBonds,
    BondPrice,
    TotalAmount,
    Status,
    StripeSessionId,
    PaymentIntentId,
    CreatedAt
FROM GiftDonations
ORDER BY CreatedAt DESC");

            return result;
        }
    }
}
