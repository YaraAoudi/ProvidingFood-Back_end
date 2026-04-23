namespace ProvidingFood2.Model
{
    public class EventDonation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EventItemId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string StripeSessionId { get; set; }
        public string PaymentIntentId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
