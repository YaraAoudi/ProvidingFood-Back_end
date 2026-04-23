using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using Stripe.Checkout;
using Stripe;
using System.Data.SqlClient;
using Dapper;

namespace ProvidingFood2.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly ICashDonationRepository _repo;
        private readonly string _connectionString;
        private readonly IChallengeService _ChallengeService;


        public PaymentService(IConfiguration config, ICashDonationRepository repo, IChallengeService challengeService)
        {
            _config = config;
            _repo = repo;
            _connectionString = config.GetConnectionString("DefaultConnection");

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            _ChallengeService = challengeService;
        }

        // ✅ جلب أو إنشاء Region
        private async Task<int> GetOrCreateRegionAsync(string regionName)
        {
            using var connection = new SqlConnection(_connectionString);

            var regionId = await connection.ExecuteScalarAsync<int?>(
                "SELECT RegionId FROM Regions WHERE Name = @Name",
                new { Name = regionName.Trim() });

            if (regionId != null) return regionId.Value;

            return await connection.ExecuteScalarAsync<int>(@"
INSERT INTO Regions (Name) VALUES (@Name);
SELECT CAST(SCOPE_IDENTITY() as int);",
                new { Name = regionName.Trim() });
        }

        // ✅ إنشاء Checkout Session
        public async Task<string> CreateCheckoutSessionAsync(CreatePaymentDto dto, int userId)
        {
            // ✅ تحقق من المدخلات
            if (dto.Amount <= 0)
                throw new Exception("المبلغ غير صالح");

            if (string.IsNullOrWhiteSpace(dto.RegionName))
                throw new Exception("اسم المنطقة مطلوب");

            var regionName = dto.RegionName.Trim();
            var regionId = await GetOrCreateRegionAsync(regionName);

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
                    UnitAmount = (long)(dto.Amount * 100), // 🔥 Stripe = cents
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "تبرع نقدي"
                    }
                },
                Quantity = 1
            }
        },

                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],

                // 🔥 أهم تعديل (لـ webhook)
                Metadata = new Dictionary<string, string>
        {
            { "type", "cash" },
            { "userId", userId.ToString() },
            { "region", regionName }
        }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // ✅ تخزين العملية
            await _repo.CreateAsync(new CashDonation
            {
                DonorUserId = userId,
                Amount = dto.Amount,
                RegionId = regionId,
                RegionName = regionName,
                StripeSessionId = session.Id,
                PaymentIntentId = null, // 🔥 بيجي لاحقاً من webhook
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            Console.WriteLine($"🔥 Session Created: {session.Id}");

            return session.Url;
        }

        public async Task<string> CreateChallengeCheckoutSessionAsync(CreatePaymentDto dto, int userId)
        {
            // ✅ تحقق من المدخلات
            if (dto.Amount <= 0)
                throw new Exception("المبلغ غير صالح");

            if (string.IsNullOrWhiteSpace(dto.RegionName))
                throw new Exception("اسم المنطقة مطلوب");

            // 🔥 تحقق: هل يوجد تحدي فعال؟
            var challenge = await _ChallengeService.GetActiveChallengeAsync();

            if (challenge == null)
                throw new Exception("❌ لا يوجد تحدي فعال حالياً");

            // 🔥 تحقق: هل تبرع اليوم؟
            var canDonate = await _ChallengeService.CanDonateToday(userId);

            if (!canDonate)
                throw new Exception("❌ يمكنك التبرع مرة واحدة فقط يومياً ضمن التحدي");

            var regionName = dto.RegionName.Trim();
            var regionId = await GetOrCreateRegionAsync(regionName);

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
                    UnitAmount = (long)(dto.Amount * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "تبرع ضمن التحدي 🔥"
                    }
                },
                Quantity = 1
            }
        },

                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],

                // 🔥 مهم جداً
                Metadata = new Dictionary<string, string>
        {
            { "type", "cash_challenge" },
            { "userId", userId.ToString() },
            { "region", regionName }
        }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // ✅ تخزين العملية
            await _repo.CreateAsync(new CashDonation
            {
                DonorUserId = userId,
                Amount = dto.Amount,
                RegionId = regionId,
                RegionName = regionName,
                StripeSessionId = session.Id,
                PaymentIntentId = null,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            Console.WriteLine($"🔥 Challenge Session Created: {session.Id}");

            return session.Url;
        }
        // ✅ Webhook
        public async Task HandleWebhookAsync(string json, string signature)
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _config["Stripe:WebhookSecret"]
            );

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;

                if (session == null)
                    return;

                var type = session.Metadata["type"];

                ///////////////////////////////////////////////////////
                // 🟢 1. التبرعات (الكود الحالي تبعك)
                ///////////////////////////////////////////////////////
                if (type == "cash")
                {
                    await _repo.UpdateStatusAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );
                }

                ///////////////////////////////////////////////////////
                // 🔵 2. دفع المطاعم (الجديد 🔥)
                ///////////////////////////////////////////////////////
                else if (type == "payout")
                {
                    var restaurantId = int.Parse(session.Metadata["restaurantId"]);

                    using var connection = new SqlConnection(_connectionString);

                    // 🔹 تحديث Payment
                    await connection.ExecuteAsync(@"
                UPDATE Payments
                SET Status = 'Paid',
                    PaymentIntentId = @IntentId
                WHERE StripeSessionId = @SessionId",
                        new
                        {
                            SessionId = session.Id,
                            IntentId = session.PaymentIntentId
                        });

                    // 🔹 حساب الرصيد
                    var balance = await connection.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(CASE 
                    WHEN Type = 'Credit' THEN Amount 
                    ELSE -Amount END),0)
                FROM RestaurantTransactions
                WHERE RestaurantId = @RestaurantId",
                        new { RestaurantId = restaurantId });

                    // 🔹 تسجيل Debit
                    if (balance > 0)
                    {
                        await connection.ExecuteAsync(@"
                    INSERT INTO RestaurantTransactions
                    (RestaurantId, Amount, Type)
                    VALUES (@RestaurantId, @Amount, 'Debit')",
                            new
                            {
                                RestaurantId = restaurantId,
                                Amount = balance
                            });
                    }
                }
            }
        }

        // (اختياري)
        public async Task HandleSuccessfulPayment(string sessionId)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);

            Console.WriteLine("SESSION ID: " + session.Id);
            Console.WriteLine("PAYMENT INTENT: " + session.PaymentIntentId);

            await _repo.UpdateStatusAsync(
                session.Id,
                session.PaymentIntentId,
                "Paid"
            );

            Console.WriteLine("🔥 دخلنا HandleSuccessfulPayment");
        }


    }
}