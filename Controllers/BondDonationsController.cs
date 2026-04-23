using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
    [Route("api/bond")]
    [ApiController]
    public class BondDonationController : ControllerBase
    {
        private readonly IBondService _service;

        public BondDonationController(IBondService service)
        {
            _service = service;
        }

        [HttpPost("create-session")]
        [Authorize]
        public async Task<IActionResult> Create(CreateBondDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var url = await _service.CreateBondSession(dto, userId);

            return Ok(new { url });
        }
    }
}
