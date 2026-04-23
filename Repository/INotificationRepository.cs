using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface INotificationRepository
    {
        Task CreateAsync(int userId, string title, string message);

        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);

        Task MarkAsReadAsync(int id);

        Task<int> GetUnreadCountAsync(int userId);
    }
}
