using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Service
{
    public class ShelterService : IShelterService
    {
        private readonly IShelterRepository _repo;
        private readonly IWebHostEnvironment _env;

        public ShelterService(IShelterRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        //////////////////////////////////////////////////////
        // 🏠 تسجيل ملجأ (رفع صورة إثبات)
        //////////////////////////////////////////////////////
        public async Task<int> RegisterAsync(RegisterShelterDto dto, int UserId)
        {
            var imagePath = await SaveImage(dto.ProofImage);

            var shelter = new Shelter
            {
                Name = dto.Name,
                Description = dto.Description,
                ProofImageUrl = imagePath,
                Status = "Pending",
                UserId = UserId // 🔥 أهم إضافة
            };

            return await _repo.CreateAsync(shelter);
        }

        //////////////////////////////////////////////////////
        // 🛡️ جلب الطلبات المعلقة
        //////////////////////////////////////////////////////
        public async Task<IEnumerable<Shelter>> GetPendingAsync()
        {
            return await _repo.GetPendingAsync();
        }

        public async Task<IEnumerable<Shelter>> GetAllrequestAsync()
        {
            return await _repo.GetAllAsync();
        }

        //////////////////////////////////////////////////////
        // 🔍 جلب حسب ID
        //////////////////////////////////////////////////////
        public async Task<Shelter> GetByIdAsync(int id)
        {
            var shelter = await _repo.GetByIdAsync(id);

            if (shelter == null)
                throw new Exception("Shelter not found");

            return shelter;
        }

        //////////////////////////////////////////////////////
        // ✅ موافقة
        //////////////////////////////////////////////////////
        public async Task ApproveAsync(int id)
        {
            var shelter = await _repo.GetByIdAsync(id);

            if (shelter == null)
                throw new Exception("Shelter not found");

            if (shelter.Status == "Approved")
                throw new Exception("Already approved");

            await _repo.ApproveAsync(id);
        }

        //////////////////////////////////////////////////////
        // ❌ رفض
        //////////////////////////////////////////////////////
        public async Task RejectAsync(int id)
        {
            var shelter = await _repo.GetByIdAsync(id);

            if (shelter == null)
                throw new Exception("Shelter not found");

            if (shelter.Status == "Rejected")
                throw new Exception("Already rejected");

            await _repo.RejectAsync(id);
        }

        //////////////////////////////////////////////////////
        // 💾 حفظ الصورة
        //////////////////////////////////////////////////////
        private async Task<string> SaveImage(IFormFile file)
        {
            if (string.IsNullOrEmpty(_env.WebRootPath))
                throw new Exception("WebRootPath is null. Make sure wwwroot exists.");

            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "shelters");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/shelters/{fileName}";
        }

       
    }
}