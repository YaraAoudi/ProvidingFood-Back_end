using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using ProvidingFood2.Service;

public class ShelterPostService
{
    private readonly IShelterPostRepository _postRepo;
    private readonly IShelterRepository _shelterRepo;
    private readonly IHttpContextAccessor _http;
    private readonly INotificationService _notificationService;
    public ShelterPostService(
        IShelterPostRepository postRepo,
        IShelterRepository shelterRepo,
        IHttpContextAccessor http ,
        INotificationService notificationService)
    {
        _postRepo = postRepo;
        _shelterRepo = shelterRepo;
        _http = http;
        _notificationService = notificationService;
    }

    public async Task<int> CreatePostAsync(CreateShelterPostDto dto, int userId)
    {
        string imageUrl = null;

        // 🔥 1. رفع الصورة
        if (dto.DisplayImage != null)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(dto.DisplayImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.DisplayImage.CopyToAsync(stream);
            }

            imageUrl = "/images/" + fileName;
        }

        // 🔥 2. جلب الملجأ كامل (مو بس ID)
        var shelter = await _postRepo.GetShelterByUserId(userId);

        if (shelter == null)
            throw new Exception("هذا المستخدم لا يملك ملجأ");

        // 🚨 3. تحقق من حالة الموافقة
        if (shelter.Status != "Approved")
            throw new Exception("الحساب غير مفعل من الإدارة ولا يمكن إنشاء بوستات");

        // 🔥 4. إنشاء البوست
        var post = new ShelterPost
        {
            ShelterId = shelter.Id, // ✔ آمن الآن
            Title = dto.Title,
            Description = dto.Description,
            RequiredMeals = dto.RequiredMeals,
            CollectedMeals = 0,
            DisplayImageUrl = imageUrl,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        return await _postRepo.CreateAsync(post);
    }
    public async Task UpdateProgressAsync(int postId, int meals, int restaurantId)
    {
        var post = await _postRepo.GetByIdAsync(postId);

        if (post == null)
            throw new Exception("Post not found");

        if (post.Status == "Completed")
            throw new Exception("تم اكتمال الطلب ولا يمكن التبرع");

        if (meals <= 0)
            throw new Exception("عدد غير صالح");

        // 🟢 1. حفظ التبرع
        await _postRepo.AddDonationAsync(postId, restaurantId, meals);

        // 🟢 2. تحديث العدد
        post.CollectedMeals += meals;

        // 🟢 3. تحقق من الاكتمال
        if (post.CollectedMeals >= post.RequiredMeals)
        {
            post.CollectedMeals = post.RequiredMeals;
            post.Status = "Completed";
        }

        await _postRepo.UpdateAsync(post);

        // 🟢 4. جلب صاحب الملجأ
        var ownerUserId = await _postRepo.GetShelterOwnerUserIdByPostId(postId);

        if (ownerUserId != 0)
        {
            var restaurantName = await _postRepo.GetRestaurantNameById(restaurantId);

            await _notificationService.SendNotificationAsync(
                ownerUserId,
                "تبرع جديد 🎉",
                $"مطعم {restaurantName} تبرع بـ {meals} وجبة"
            );
        }
    }
    public async Task<IEnumerable<ShelterPostWithDonationsDto>> GetAllPosts()
    {
        return await _postRepo.GetPostsWithDonations();
    }

    public async Task<int> GetRestaurantIdByUserId(int userId)
    {
        var restaurantId = await _postRepo.GetRestaurantIdByUserId(userId);

        if (restaurantId == null)
            throw new Exception("هذا المستخدم ليس لديه مطعم");

        return restaurantId.Value;
    }
}