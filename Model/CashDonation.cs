using System;

namespace ProvidingFood2.Model
{
    public class CashDonation
    {
        public int DonationId { get; set; }          // معرف التبرع (Primary Key)
        public int DonorUserId { get; set; }         // معرف المستخدم المتبرع (FK لـ User)
        public decimal Amount { get; set; }          // قيمة التبرع
        public int? RegionId { get; set; }           // معرف المنطقة (FK لـ Regions)
        public string RegionName { get; set; }       // اسم المنطقة (snapshot)
        public string StripeSessionId { get; set; }  // معرف جلسة Stripe
        public string PaymentIntentId { get; set; }  // معرف الدفع من Stripe
        public string Status { get; set; }           // Pending / Paid / Failed
        public DateTime CreatedAt { get; set; }      // وقت إنشاء التبرع
    }
}