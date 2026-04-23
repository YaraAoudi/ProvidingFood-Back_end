using ProvidingFood2.DTO;
using ProvidingFood2.Model;

namespace ProvidingFood2.Service
{
	public interface IBeneficiaryService
	{
        Task<RequestResult> SubmitRequestAsync(BeneficiaryRequestDto dto, int userId);

        Task<RequestResult> ApproveRequestAsync(int requestId);
		Task<RequestResult> RejectRequestAsync(int requestId);
		Task<IEnumerable<BeneficiaryRequest>> GetPendingRequestsAsync();
		Task<IEnumerable<BeneficiaryRequest>> GetAllRequestsAsync();
		Task<IEnumerable<BeneficiaryRequestDto2>> GetRequestsByUserEmailAsync(string email);

    }
}
