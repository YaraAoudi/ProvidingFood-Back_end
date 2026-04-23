using ProvidingFood2.DTO;
using ProvidingFood2.Repository;
using Stripe.Checkout;
using Stripe;
using ProvidingFood2.Model;

namespace ProvidingFood2.Service
{
    public class GiftService : IGiftService
    {
        private readonly IBondRepository _bondRepo;
        private readonly IGiftDonationRepository _giftRepo;
        private readonly IConfiguration _config;
        private readonly IChallengeService _challengeService;
        public GiftService(IBondRepository bondRepo, IGiftDonationRepository giftRepo, IConfiguration config , IChallengeService ChallengeService)
        {
            _bondRepo = bondRepo;
            _giftRepo = giftRepo;
            _config = config;
            _challengeService = ChallengeService;

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<string> CreateGiftSession(CreateGiftDonationDto dto, int userId)
        {
            if (dto.NumberOfBonds <= 0)
                throw new Exception("عدد السندات غير صالح");

            var bondPrice = await _bondRepo.GetActiveBondPriceAsync();
            var total = dto.NumberOfBonds * bondPrice;

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
                        Name = "إهداء سندات طعام"
                    }
                },
                Quantity = 1
            }
        },

                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],

                // 🔥🔥🔥 أهم إضافة
                Metadata = new Dictionary<string, string>
        {
            { "type", "gift" },
            { "userId", userId.ToString() }
        }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            await _giftRepo.CreateAsync(new GiftDonation
            {
                DonorUserId = userId,
                RecipientName = dto.RecipientName,
                RecipientPhone = dto.RecipientPhone,
                RecipientAddress = dto.RecipientAddress,
                NumberOfBonds = dto.NumberOfBonds,
                BondPrice = bondPrice,
                TotalAmount = total,
                StripeSessionId = session.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            return session.Url;
        }

        public async Task<string> CreateGiftChallengeSession(CreateGiftDonationDto dto, int userId)
        {
            if (dto.NumberOfBonds <= 0)
                throw new Exception("عدد السندات غير صالح");

            // 🔥 تحقق من وجود تحدي
            var challenge = await _challengeService.GetActiveChallengeAsync();

            if (challenge == null)
                throw new Exception("❌ لا يوجد تحدي فعال حالياً");

            // 🔥 تحقق من التبرع اليومي
            var canDonate = await _challengeService.CanDonateToday(userId);

            if (!canDonate)
                throw new Exception("❌ يمكنك الإهداء مرة واحدة فقط يومياً ضمن التحدي");

            var bondPrice = await _bondRepo.GetActiveBondPriceAsync();
            var total = dto.NumberOfBonds * bondPrice;

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
                        Name = "إهداء سندات ضمن التحدي 🔥"
                    }
                },
                Quantity = 1
            }
        },

                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],

                // 🔥 أهم نقطة
                Metadata = new Dictionary<string, string>
        {
            { "type", "gift_challenge" },
            { "userId", userId.ToString() }
        }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            await _giftRepo.CreateAsync(new GiftDonation
            {
                DonorUserId = userId,
                RecipientName = dto.RecipientName,
                RecipientPhone = dto.RecipientPhone,
                RecipientAddress = dto.RecipientAddress,
                NumberOfBonds = dto.NumberOfBonds,
                BondPrice = bondPrice,
                TotalAmount = total,
                StripeSessionId = session.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            Console.WriteLine($"🔥 Gift Challenge Session: {session.Id}");

            return session.Url;
        }
    }
}