using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IStoreRepository
	{
		Task<int> CreateRequestAsync(StoreDonationRequest request);
		Task<IEnumerable<StoreDonationRequest>> GetByStoreUserIdAsync(int storeUserId);

		Task<IEnumerable<StoreDonationRequest>> GetPendingAsync();
		Task<IEnumerable<StoreDonationRequest>> GetAllRequestAsync();

        Task<StoreDonationRequest?> GetByIdAsync(int id);
		Task ApproveAsync(int id);
        Task RejectAsync(int id);

    }
}
