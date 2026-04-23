using ProvidingFood2.Model;

namespace ProvidingFood2.Service
{
	public interface IStoreService
	{
		Task<int> SubmitRequestAsync(StoreDonationRequest request, int storeUserId);
		Task<IEnumerable<StoreDonationRequest>> GetMyRequestsAsync(int storeUserId);

		Task<IEnumerable<StoreDonationRequest>> GetPendingRequestsAsync();
		Task<IEnumerable<StoreDonationRequest>> GetRequestsAsync();

        Task ApproveRequestAsync(int requestId);
        Task RejectAsync(int id);
    }
}
