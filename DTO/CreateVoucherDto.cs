namespace ProvidingFood2.DTOs
{
	public class CreateVoucherDto
	{
		public int BeneficiaryId { get; set; }
		public string BeneficiaryName { get; set; }
		public string StoreName { get; set; }
		public string StoreLocation { get; set; }
		public int BasketCount { get; set; }
		public DateTime ExpiryDate { get; set; }
	}
}