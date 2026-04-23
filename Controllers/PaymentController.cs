using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using System.Security.Claims;
using Stripe.Checkout;
using Stripe;
using ProvidingFood2.Repository;
using System.Data.SqlClient;
using Dapper;
namespace ProvidingFood2.Controllers
{


    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IConfiguration _configuration;
        private readonly ICashDonationRepository _cashRepo;
        private readonly IGiftDonationRepository _giftRepo;
        private readonly IBondDonationRepository _bondDonationRepo;
        private readonly IChallengeService _challengeService;
        private readonly IEventDonationRepository _eventDonationRepo;
        public PaymentController(IPaymentService service, IConfiguration configuration, ICashDonationRepository cashRepo,
        IGiftDonationRepository giftRepo , IBondDonationRepository bondDonationRepo, IChallengeService challengeService , IEventDonationRepository eventDonationRepo)
        {
            _service = service;
            _configuration = configuration;
            _configuration = configuration;
            _cashRepo = cashRepo;
            _giftRepo = giftRepo;
            _bondDonationRepo = bondDonationRepo;
            _challengeService = challengeService;
            _eventDonationRepo = eventDonationRepo;

        }

        [HttpPost("create-session")]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> CreateSession(CreatePaymentDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("UserId not found in token");

            var userId = int.Parse(userIdClaim.Value);
            var url = await _service.CreateCheckoutSessionAsync(dto, userId);
            return Ok(new { url });
        }

        [HttpPost("create-challenge-session")]
        [Authorize]
        public async Task<IActionResult> CreateChallenge(CreatePaymentDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var url = await _service.CreateChallengeCheckoutSessionAsync(dto, userId);

            return Ok(new { url });
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                Request.EnableBuffering();

                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Request.Body.Position = 0;

                var signature = Request.Headers["Stripe-Signature"];

                Event stripeEvent;

                try
                {
                    stripeEvent = EventUtility.ConstructEvent(
                        json,
                        signature,
                        _configuration["Stripe:WebhookSecret"]
                    );
                }
                catch
                {
                    return Ok(); // مهم Stripe ما ياخد error
                }

                if (stripeEvent.Type != "checkout.session.completed")
                {
                    return Ok();
                }

                var session = stripeEvent.Data.Object as Session;

                if (session == null)
                    return Ok();

                // تأكد من metadata
                if (session.Metadata == null || !session.Metadata.ContainsKey("type"))
                    return Ok();

                var type = session.Metadata["type"];

                ///////////////////////////////////////////////////////
                //  CASH (التبرعات النقدية)
                ///////////////////////////////////////////////////////
                if (type == "cash")
                {
                    await _cashRepo.UpdateStatusAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );
                }

                ///////////////////////////////////////////////////////
                // 🔥 CASH CHALLENGE (تبرع داخل التحدي)
                ///////////////////////////////////////////////////////
                else if (type == "cash_challenge")
                {
                    Console.WriteLine("🔥 Challenge Cash Payment");

                    await _cashRepo.UpdateStatusAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );

                    if (session.Metadata.ContainsKey("userId"))
                    {
                        var userId = int.Parse(session.Metadata["userId"]);

                        Console.WriteLine($"🔥 UserId: {userId}");
                        Console.WriteLine($"🔥 SessionId: {session.Id}");

                        await _challengeService.AddPointAsync(userId, session.Id);
                    }
                }

                ///////////////////////////////////////////////////////
                //  GIFT
                ///////////////////////////////////////////////////////
                else if (type == "gift")
                {
                    await _giftRepo.UpdateStatusGiftAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );
                }

                ///////////////////////////////////////////////////////
                // 🎁 GIFT CHALLENGE
                ///////////////////////////////////////////////////////
                else if (type == "gift_challenge")
                {
                    Console.WriteLine("🔥 Gift Challenge Payment");

                    await _giftRepo.UpdateStatusGiftAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );

                    if (session.Metadata.ContainsKey("userId"))
                    {
                        var userId = int.Parse(session.Metadata["userId"]);

                        await _challengeService.AddPointAsync(userId, session.Id);
                    }
                }

                else if (type == "event")
                {
                    await _eventDonationRepo.UpdateStatusAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );
                }

                ///////////////////////////////////////////////////////
                // 🟡 BOND
                ///////////////////////////////////////////////////////
                else if (type == "bond") // 🔥 صارت else if
                {
                    Console.WriteLine("🔥 Bond Payment");

                    await _bondDonationRepo.UpdateStatusAsync(
                        session.Id,
                        session.PaymentIntentId,
                        "Paid"
                    );

                    if (session.Metadata.ContainsKey("userId"))
                    {
                        var userId = int.Parse(session.Metadata["userId"]);

                        Console.WriteLine($"🔥 UserId: {userId}");
                        Console.WriteLine($"🔥 SessionId: {session.Id}");

                        await _challengeService.AddPointAsync(userId, session.Id);
                    }
                }

                ///////////////////////////////////////////////////////
                //  PAYOUT (دفع المطاعم )
                ///////////////////////////////////////////////////////
                else if (type == "payout")
                {
                    if (!session.Metadata.ContainsKey("restaurantId"))
                        return Ok();

                    var restaurantId = int.Parse(session.Metadata["restaurantId"]);

                    using var connection = new SqlConnection(
                        _configuration.GetConnectionString("DefaultConnection"));

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

                    var balance = await connection.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(CASE 
                    WHEN Type = 'Credit' THEN Amount 
                    ELSE -Amount END),0)
                FROM RestaurantTransactions
                WHERE RestaurantId = @RestaurantId",
                        new { RestaurantId = restaurantId });

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

                return Ok();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("error.txt", ex.ToString());
                throw;
            }
        }
    }
}
