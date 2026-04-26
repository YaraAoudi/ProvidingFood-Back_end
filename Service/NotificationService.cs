using Microsoft.AspNetCore.SignalR;
using ProvidingFood2.Hubs;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using ProvidingFood2.Service;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly INotificationRepository _repo;

    public NotificationService(IHubContext<NotificationHub> hub, INotificationRepository repo)
    {
        _hub = hub;
        _repo  = repo;
    }

    public async Task SendNotificationAsync(int userId, string title, string message)
    {
        Console.WriteLine("=== SENDING NOTIFICATION ===");
        Console.WriteLine("To Group: " + userId);
        Console.WriteLine("Title: " + title);
        Console.WriteLine("Message: " + message);

        await _repo.CreateAsync(userId, title, message);

        await _hub.Clients.User(userId.ToString())
     .SendAsync("ReceiveNotification", new
     {
         title,
         message
     });
        Console.WriteLine("Notification sent via SignalR");
    }
    public async Task<IEnumerable<Notification>> GetMyNotifications(int userId)
    {
        return await _repo.GetByUserIdAsync(userId);
    }
}