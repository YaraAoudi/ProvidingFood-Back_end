using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using Stripe;
using Stripe.Checkout;

namespace ProvidingFood2.Service
{
    public class BondService : IBondService
    {
        private readonly IBondRepository _bondRepo;
        private readonly IConfiguration _config;
        private readonly IBondDonationRepository _bondDonationRepo;
        private readonly IChallengeService _challengeService;

        public BondService(IBondRepository bondRepo, IConfiguration config, IBondDonationRepository bondDonationRepo , IChallengeService challengeService) {
            _bondRepo = bondRepo;
             _config = config;
            _bondDonationRepo = bondDonationRepo;
            _challengeService = challengeService;
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }
        public async Task<string> CreateBondSession(CreateBondDto dto, int userId)
        {
            // 🔥 تحقق قبل الدفع
            var canDonate = await _challengeService.CanDonateToday(userId);

            if (!canDonate)
                throw new Exception("❌ لا يمكنك التبرع أكثر من مرة اليوم أو لا يوجد تحدي فعال");

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
                        Name = "سندات طعام للجمعية"
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
            { "type", "bond" },
            { "userId", userId.ToString() }
        }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            await _bondDonationRepo.CreateAsync(new BondDonation
            {
                DonorUserId = userId,
                NumberOfBonds = dto.NumberOfBonds,
                BondPrice = bondPrice,
                TotalAmount = total,
                StripeSessionId = session.Id,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            return session.Url;
        }

    }
}
