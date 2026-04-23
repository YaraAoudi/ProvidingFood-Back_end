using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Service
{
	public class StoreService : IStoreService
	{
		private readonly IStoreRepository _repository;

		public StoreService(IStoreRepository repository)
		{
			_repository = repository;
		}

		public async Task<int> SubmitRequestAsync(StoreDonationRequest request, int storeUserId)
		{
			if (request.BasketCount <= 0)
				throw new Exception("عدد السلات يجب أن يكون أكبر من صفر");

			request.StoreUserId = storeUserId;
			request.Status = "Pending";
			request.CreatedAt = DateTime.UtcNow;

			return await _repository.CreateRequestAsync(request);
		}

		public async Task<IEnumerable<StoreDonationRequest>> GetMyRequestsAsync(int storeUserId)
		{
			return await _repository.GetByStoreUserIdAsync(storeUserId);
		}

		// ✅ عرض الطلبات المعلقة
		public async Task<IEnumerable<StoreDonationRequest>> GetPendingRequestsAsync()
		{
			return await _repository.GetPendingAsync();
		}
        public async Task<IEnumerable<StoreDonationRequest>> GetRequestsAsync()
        {
            return await _repository.GetAllRequestAsync();
        }

        // ✅ موافقة الأدمن
        public async Task ApproveRequestAsync(int requestId)
		{
			var request = await _repository.GetByIdAsync(requestId);

			if (request == null)
				throw new Exception("الطلب غير موجود");

			if (request.Status != "Pending")
				throw new Exception("لا يمكن الموافقة على هذا الطلب");

			await _repository.ApproveAsync(requestId);
		}
        public async Task RejectAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);

            if (request == null)
                throw new Exception("الطلب غير موجود");

            if (request.Status != "Pending")
                throw new Exception("لا يمكن الموافقة على هذا الطلب");

            await _repository.RejectAsync(id);
        }
    }
}
