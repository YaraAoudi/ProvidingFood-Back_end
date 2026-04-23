namespace ProvidingFood2.Model
{
    public class GiftDonation
    {
        public int GiftDonationId { get; set; }

        // 👤 المتبرع
        public int DonorUserId { get; set; }

        // 👨‍👩‍👧‍👦 الشخص / العائلة المستفيدة
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientAddress { get; set; }

        // 🎁 تفاصيل السند
        public int NumberOfBonds { get; set; }   // عدد السندات
        public decimal BondPrice { get; set; } // سعر السند الواحد
        public decimal TotalAmount { get; set; } // = العدد × السعر

        // 🌍 المنطقة
        public int RegionId { get; set; }
        public string RegionName { get; set; }

        // 💳 Stripe
        public string StripeSessionId { get; set; }
        public string PaymentIntentId { get; set; }

        // 📌 الحالة
        public string Status { get; set; } // Pending / Paid

        // ⏱️ تاريخ
        public DateTime CreatedAt { get; set; }
    }
}