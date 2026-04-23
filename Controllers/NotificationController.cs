using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Service;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
   

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.GetUserId(); // 👈 هون

            var data = await _service.GetMyNotifications(userId);

            return Ok(data);
        }
    }
}
