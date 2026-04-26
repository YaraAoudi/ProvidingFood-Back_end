using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProvidingFood2.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine("=== SIGNALR CONNECTED ===");
            Console.WriteLine("UserId: " + userId);
            Console.WriteLine("ConnectionId: " + Context.ConnectionId);

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ userId is NULL");
            }

            await base.OnConnectedAsync();
        }
    }
}