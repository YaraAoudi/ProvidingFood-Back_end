using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
    [Route("api/gift")]
    [ApiController]
    public class GiftController : ControllerBase
    {
        private readonly IGiftService _service;

        public GiftController(IGiftService service)
        {
            _service = service;
        }

        [HttpPost("create-session")]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> Create(CreateGiftDonationDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var url = await _service.CreateGiftSession(dto, userId);

            return Ok(new { url });
        }

        [HttpPost("create-gift-challenge-session")]
        [Authorize(Roles = "Donor")]

        public async Task<IActionResult> CreateGiftChallenge(CreateGiftDonationDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var url = await _service.CreateGiftChallengeSession(dto, userId);

            return Ok(new { url });
        }
    }
}
