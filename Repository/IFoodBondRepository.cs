using ProvidingFood2.DTO;

namespace ProvidingFood2.Repository
{
	public interface IFoodBondRepository
	{
		Task<QRScanResult> ScanQRCodeAsync(string qrCode);
		Task<bool> UpdateBondStatusAsync(int bondId, string newStatus);
		Task CheckAndExpireBondsAsync();
		Task<int> CreateFoodBondAsync(FoodBondCreateRequest request);
		Task<FoodBondResponse?> GetFoodBondByIdAsync(int id);
		Task<IEnumerable<FoodBondResponse>> GetAllFoodBondsAsync();
		Task ConfirmBondReceiptAsync(int bondId);
		Task<decimal> GetRestaurantBalance(int restaurantId);

    }
}
