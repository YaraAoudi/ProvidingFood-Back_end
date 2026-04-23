using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class GiftDonationRepository : IGiftDonationRepository
    {
        private readonly string _connectionString;

        public GiftDonationRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task CreateAsync(GiftDonation d)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
        INSERT INTO GiftDonations
        (DonorUserId, RecipientName, RecipientPhone, RecipientAddress,
         NumberOfBonds, BondPrice, TotalAmount,
         StripeSessionId, Status, CreatedAt)
        VALUES
        (@DonorUserId, @RecipientName, @RecipientPhone, @RecipientAddress,
         @NumberOfBonds, @BondPrice, @TotalAmount,
         @StripeSessionId, @Status, @CreatedAt)", d);
        }

        public async Task<int> UpdateStatusGiftAsync(string sessionId, string paymentIntentId, string status)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(@"
UPDATE GiftDonations
SET 
    Status = @Status,
    PaymentIntentId = COALESCE(PaymentIntentId, @PaymentIntentId)
WHERE 
    StripeSessionId = @SessionId
    OR PaymentIntentId = @PaymentIntentId",
            new
            {
                SessionId = sessionId,
                PaymentIntentId = paymentIntentId,
                Status = status
            });

            // 🔥 مهم جدًا للـ debugging
            System.IO.File.AppendAllText("log.txt", $"🎁 Gift Rows Updated: {rows}\n");

            return rows;
        }
    }
}
