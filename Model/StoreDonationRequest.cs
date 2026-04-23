namespace ProvidingFood2.Model
{
	public class StoreDonationRequest
	{
		public int RequestId { get; set; }
		public int StoreUserId { get; set; }

		public string StoreName { get; set; }
		public string StoreLocation { get; set; }
		public string PhoneNumber { get; set; }

		public int BasketCount { get; set; }
		public string BasketContents { get; set; }

		public string Status { get; set; } = "Pending";
		public DateTime CreatedAt { get; set; }
	}
}
