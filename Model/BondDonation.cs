using System;

namespace ProvidingFood2.Model
{
    public class BondDonation
    {
        public int Id { get; set; }

        // 👤 المتبرع
        public int DonorUserId { get; set; }

        // 📦 عدد السندات
        public int NumberOfBonds { get; set; }

        // 💲 سعر السند الواحد
        public decimal BondPrice { get; set; }

        // 💰 المبلغ الكلي
        public decimal TotalAmount { get; set; }

        // 🔗 Stripe
        public string StripeSessionId { get; set; }
        public string PaymentIntentId { get; set; }

        // 📊 حالة الدفع
        public string Status { get; set; } // Pending / Paid / Failed

        // 📅 تاريخ الإنشاء
        public DateTime CreatedAt { get; set; }

        // 🏢 هل تم توزيع السندات؟
        public bool Assigned { get; set; } = false;
    }
}