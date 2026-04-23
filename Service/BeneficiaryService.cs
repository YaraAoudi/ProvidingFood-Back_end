using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Service
{
	public class BeneficiaryService : IBeneficiaryService
	{
		private readonly IBeneficiaryRepository _repository;

		public BeneficiaryService(IBeneficiaryRepository repository)
		{
			_repository = repository;
		}

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/" + fileName; 
        }
        public async Task<RequestResult> SubmitRequestAsync(BeneficiaryRequestDto dto, int userId)
        {
            var maritalImage = await SaveFileAsync(dto.MaritalStatusProofImage);
            var familyImage = await SaveFileAsync(dto.FamilySizeProofImage);

            var entity = new BeneficiaryRequest
            {
                UserId = userId, // 👈 من التوكن
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                FamilySize = dto.FamilySize,
                MaritalStatus = dto.MaritalStatus,
                MaritalStatusProofImage = maritalImage,
                FamilySizeProofImage = familyImage,
                Status = RequestStatus.Pending
            };

            var id = await _repository.SubmitRequestAsync(entity);

            return new RequestResult
            {
                Success = true,
                Id = id,
                Message = "تم إرسال الطلب وهو قيد المراجعة"
            };
        }

        public async Task<RequestResult> ApproveRequestAsync(int requestId)
		{
			var request = await _repository.GetPendingRequestAsync(requestId);

			if (request == null)
			{
				return new RequestResult
				{
					Success = false,
					Message = "الطلب غير موجود أو تمت معالجته مسبقاً"
				};
			}

			await _repository.InsertBeneficiaryAsync(request);
			await _repository.UpdateRequestStatusAsync(requestId, "Approved");

			return new RequestResult
			{
				Success = true,
				Message = "تمت الموافقة وإضافة المستفيد"
			};
		}

		public async Task<RequestResult> RejectRequestAsync(int requestId)
		{
			var rows = await _repository.RejectRequestAsync(requestId);

			if (rows == 0)
			{
				return new RequestResult
				{
					Success = false,
					Message = "الطلب غير موجود"
				};
			}

			return new RequestResult
			{
				Success = true,
				Message = "تم رفض الطلب"
			};
		}

		public Task<IEnumerable<BeneficiaryRequest>> GetPendingRequestsAsync()
			=> _repository.GetPendingRequestsAsync();

		public Task<IEnumerable<BeneficiaryRequest>> GetAllRequestsAsync()
			=> _repository.GetAllRequestsAsync();



        public async Task<IEnumerable<BeneficiaryRequestDto2>> GetRequestsByUserEmailAsync(string email)
        {
            var requests = await _repository.GetRequestsByUserEmailAsync(email);

            return requests.Select(x => new BeneficiaryRequestDto2
            {
                RequestId = x.RequestId,
                FullName = x.FullName,
                PhoneNumber = x.PhoneNumber,
                FamilySize = x.FamilySize,
                MaritalStatus = x.MaritalStatus,

                MaritalStatusProofImage = x.MaritalStatusProofImage,
                FamilySizeProofImage = x.FamilySizeProofImage,

                Status = x.Status switch
                {
                    RequestStatus.Pending => "قيد الانتظار",
                    RequestStatus.Approved => "تمت الموافقة",
                    RequestStatus.Rejected => "مرفوض",
                    _ => x.Status.ToString()
                }
            });
        }
    }

}
