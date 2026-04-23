using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ProvidingFood2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShelterController : ControllerBase
    {
      
        private readonly IShelterService _service;

        public ShelterController(IShelterService service)
        {
            _service = service;
        }

        //////////////////////////////////////////////////////
        // 🏠 تسجيل ملجأ (طلب توثيق)
        //////////////////////////////////////////////////////
        [HttpPost("register")]
        [Authorize] // 🔥 مهم جداً
        public async Task<IActionResult> Register([FromForm] RegisterShelterDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null)
                return Unauthorized("UserId not found in token");

            var userId = int.Parse(userIdClaim.Value);

            var id = await _service.RegisterAsync(dto, userId);

            return Ok(new { shelterId = id });
        }

        //////////////////////////////////////////////////////
        // 🛡️ عرض الطلبات المعلقة (للأدمن)
        //////////////////////////////////////////////////////
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var data = await _service.GetPendingAsync();

            return Ok(data.Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.ProofImageUrl, 
                s.Status
            }));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllrequestAsync();

            return Ok(data.Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.ProofImageUrl, 
                s.Status
            }));
        }

        //////////////////////////////////////////////////////
        // ✅ موافقة
        //////////////////////////////////////////////////////
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            await _service.ApproveAsync(id);
            return Ok(new { message = "Shelter approved" });
        }

        //////////////////////////////////////////////////////
        // ❌ رفض
        //////////////////////////////////////////////////////
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            await _service.RejectAsync(id);
            return Ok(new { message = "Shelter rejected" });
        }

        //////////////////////////////////////////////////////
        // 🔍 جلب ملجأ حسب ID
        //////////////////////////////////////////////////////
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var shelter = await _service.GetByIdAsync(id);

            if (shelter == null)
                return NotFound();

            return Ok(shelter);
        }


    }
}