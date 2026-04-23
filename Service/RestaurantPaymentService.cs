using Dapper;
using Stripe.Checkout;
using Stripe;
using System.Data.SqlClient;

namespace ProvidingFood2.Service
{
    public class RestaurantPaymentService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public RestaurantPaymentService(IConfiguration config)
        {
            _config = config;
            _connectionString = config.GetConnectionString("DefaultConnection");

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<string> CreateRestaurantPayment(int restaurantId)
        {
            using var connection = new SqlConnection(_connectionString);

            // 🔹 حساب الرصيد
            var balance = await connection.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(CASE 
                    WHEN Type = 'Credit' THEN Amount 
                    ELSE -Amount END),0)
                FROM RestaurantTransactions
                WHERE RestaurantId = @RestaurantId",
                new { RestaurantId = restaurantId });

            if (balance <= 0)
                throw new Exception("لا يوجد رصيد");

            // 🔹 إنشاء Stripe Session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],

                Metadata = new Dictionary<string, string>
                {
                    { "type", "payout" },
                    { "restaurantId", restaurantId.ToString() }
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(balance * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Restaurant Payment"
                            }
                        },
                        Quantity = 1
                    }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // 🔹 تخزين العملية
            await connection.ExecuteAsync(@"
                INSERT INTO Payments (RestaurantId, Amount, StripeSessionId, Status, Type)
                VALUES (@RestaurantId, @Amount, @SessionId, 'Pending', 'payout')",
                new
                {
                    RestaurantId = restaurantId,
                    Amount = balance,
                    SessionId = session.Id
                });

            return session.Url;
        }
    }
}