namespace ProvidingFood2.Model
{
	public class Voucher
	{
		public int VoucherId { get; set; }

		public int BeneficiaryId { get; set; }
		public string BeneficiaryName { get; set; }

		public string StoreName { get; set; }
		public string StoreLocation { get; set; }

		public int BasketCount { get; set; }

		public DateTime ExpiryDate { get; set; }

		public string QRCode { get; set; }

		public string Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? UsedAt { get; set; }
	}
}