using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using Stripe.Checkout;
using Stripe;
using System.Data.SqlClient;
using Dapper;

namespace ProvidingFood2.Service
{
    public class EventDonationService
    {
        private readonly IConfiguration _config;
        private readonly IEventDonationRepository _repo;
        private readonly string _connectionString;

        public EventDonationService(IConfiguration config, IEventDonationRepository repo)
        {
            _config = config;
            _repo = repo;
            _connectionString = config.GetConnectionString("DefaultConnection");

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<string> CreateSession(CreateEventDonationDto dto, int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            // 🔥 جيب الـ item
            var item = await connection.QueryFirstOrDefaultAsync<EventItem>(@"
            SELECT * FROM EventItems WHERE Id = @Id",
                new { Id = dto.EventItemId });

            if (item == null)
                throw new Exception("Item not found");

            var total = item.Price * dto.Quantity;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },

                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(total * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name
                        }
                    },
                    Quantity = 1
                }
            },

                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],

                Metadata = new Dictionary<string, string>
            {
                { "type", "event" },
                { "userId", userId.ToString() }
            }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            await _repo.CreateAsync(new EventDonation
            {
                UserId = userId,
                EventItemId = dto.EventItemId,
                Quantity = dto.Quantity,
                TotalAmount = total,
                StripeSessionId = session.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            return session.Url;
        }
    }
}
