using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
    [Route("api/challenge")]
    [ApiController]
   
    public class ChallengeController : ControllerBase
    {
        private readonly ChallengeDisplayService _service;
        private readonly IChallengeService _service2;
        public ChallengeController(ChallengeDisplayService service, IChallengeService service2)
        {
            _service = service;
            _service2 = service2;
        }

        [HttpGet("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatus()
        {
            var result = await _service.GetChallengeStatus();
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateChallengeDto dto)
        {
            await _service2.CreateAsync(dto);
            return Ok("Challenge created");
        }

        [HttpPost("{id}/end")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> End(int id)
        {
            await _service2.EndAsync(id);
            return Ok("Challenge ended");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service2.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service2.GetByIdAsync(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateChallengeDto dto)
        {
            await _service2.UpdateAsync(id, dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service2.DeleteAsync(id);
            return Ok("Deleted");
        }

        [HttpGet("{id}/leaderboard")]
        public async Task<IActionResult> Leaderboard(int id)
        {
            var data = await _service2.GetLeaderboardAsync(id);
            return Ok(data);
        }
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id, ActivateChallengeDto dto)
        {
            await _service2.ActivateAsync(id, dto);
            return Ok("Challenge activated with dates");
        }



        [HttpGet("my-points")]
        public async Task<IActionResult> GetMyPoints()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                             ?? User.FindFirst("sub");

            var userId = int.Parse(userIdClaim.Value);

            var result = await _service2.GetUserPointsWithStatus(userId);

            return Ok(result);
        }
    }
}