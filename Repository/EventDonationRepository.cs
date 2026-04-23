using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class EventDonationRepository : IEventDonationRepository
    {
        private readonly string _connectionString;

        public EventDonationRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task CreateAsync(EventDonation donation)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
            INSERT INTO EventDonations
            (UserId, EventItemId, Quantity, TotalAmount, StripeSessionId, Status, CreatedAt)
            VALUES
            (@UserId, @EventItemId, @Quantity, @TotalAmount, @StripeSessionId, @Status, @CreatedAt)",
                donation);
        }

        public async Task UpdateStatusAsync(string sessionId, string intentId, string status)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
            UPDATE EventDonations
            SET PaymentIntentId = @IntentId,
                Status = @Status
            WHERE StripeSessionId = @SessionId",
                new
                {
                    SessionId = sessionId,
                    IntentId = intentId,
                    Status = status
                });
        }
      
    }
}
