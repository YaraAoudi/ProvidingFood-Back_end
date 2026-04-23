using ProvidingFood2.Model;

namespace ProvidingFood2.Service
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message);
        Task<IEnumerable<Notification>> GetMyNotifications(int userId);
    }
}
