using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventDonationController : ControllerBase
    {
        private readonly EventDonationService _service;

        public EventDonationController(EventDonationService service)
        {
            _service = service;
        }

        [HttpPost("create-session")]
        [Authorize]
        public async Task<IActionResult> Create(CreateEventDonationDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var url = await _service.CreateSession(dto, userId);

            return Ok(new { url });
        }

    }
}
