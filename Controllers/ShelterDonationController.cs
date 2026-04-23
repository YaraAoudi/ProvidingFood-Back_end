using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Model;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
    [ApiController]
    [Route("api/shelter-donations")]
    public class ShelterDonationController : ControllerBase
    {
        private readonly ShelterPostService _service;

        public ShelterDonationController(ShelterPostService service)
        {
            _service = service;
        }

        [HttpPost("donate")]
        [Authorize(Roles = "Restaurant")]
        public async Task<IActionResult> Donate([FromBody] DonateMealsDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var restaurantId = await _service.GetRestaurantIdByUserId(userId);

            await _service.UpdateProgressAsync(dto.PostId, dto.Meals, restaurantId);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllPosts();
            return Ok(data);
        }
    }
}
