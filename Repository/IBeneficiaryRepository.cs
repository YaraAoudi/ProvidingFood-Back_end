using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IBeneficiaryRepository
	{
		Task<int> SubmitRequestAsync(BeneficiaryRequest request);
		Task<BeneficiaryRequest?> GetPendingRequestAsync(int requestId);
		Task InsertBeneficiaryAsync(BeneficiaryRequest request);
		Task UpdateRequestStatusAsync(int requestId, string status);
		Task<int> RejectRequestAsync(int requestId);
		Task<IEnumerable<BeneficiaryRequest>> GetPendingRequestsAsync();
		Task<IEnumerable<BeneficiaryRequest>> GetAllRequestsAsync();
		Task<bool> DeleteBeneficiariesUserAsync(int BeneficiaryId);
		Task<bool> UpdateBeneficiaryAsync(Beneficiary newBeneficiary);
		Task<IEnumerable<BeneficiaryRequest>> GetRequestsByUserEmailAsync(string email);

    }
}
