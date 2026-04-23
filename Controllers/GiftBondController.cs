using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftBondController : ControllerBase
    {
        private readonly IBondRepository _bondRepo;

        public GiftBondController(IBondRepository bondRepo)
        {
            _bondRepo = bondRepo;
        }

        // ✅ جلب السعر الحالي
        [HttpGet("active-price")]
        [Authorize(Roles = "Donor")]
        public async Task<IActionResult> GetActiveBondPrice()
        {
            var price = await _bondRepo.GetActiveBondPriceAsync();

            if (price == 0)
                return NotFound("لا يوجد سعر مفعل حالياً");

            return Ok(new { price });
        }
        [HttpPost("set-price")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetBondPrice([FromBody] decimal price)
        {
            if (price <= 0)
                return BadRequest("السعر غير صالح");

            await _bondRepo.SetNewBondPriceAsync(price);

            return Ok(new { message = "تم تحديث السعر بنجاح" });
        }
        [HttpGet("all gift donations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllGiftDonations()
        {
            var data = await _bondRepo.GetAllAsync();

            return Ok(data);
        }
    }
}
