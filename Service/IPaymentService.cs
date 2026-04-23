using ProvidingFood2.DTO;
using Stripe;
using Stripe.Checkout;
namespace ProvidingFood2.Service
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(CreatePaymentDto dto, int userId);
        Task HandleWebhookAsync(string json, string signature);
        Task HandleSuccessfulPayment(string sessionId);
        Task<string> CreateChallengeCheckoutSessionAsync(CreatePaymentDto dto, int userId);
    }
}
