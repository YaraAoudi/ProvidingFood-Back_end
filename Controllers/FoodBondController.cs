using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodBondController : ControllerBase
    {
        private readonly IFoodBondRepository _repository;

        public FoodBondController(IFoodBondRepository repository)
        {
            _repository = repository;
        }

        /////////////////////////////////// Scan QR /////////////////////////////////
        [HttpPost("scan")]
        public async Task<IActionResult> ScanQRCode([FromBody] ScanRequest request)
        {
            try
            {
                var result = await _repository.ScanQRCodeAsync(request.QRCode);

                if (result.Status == "Expired")
                    return BadRequest(new { Error = "انتهت صلاحية السند" });

                if (result.Status == "Received")
                    return BadRequest(new { Error = "تم استخدام السند مسبقاً" });

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        /////////////////////////////////// Confirm Receipt 🔥 /////////////////////////////////
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmReceipt([FromBody] ConfirmRequest request)
        {
            try
            {
                await _repository.ConfirmBondReceiptAsync(request.BondId);

                return Ok(new
                {
                    Message = "تم تأكيد الاستلام وتسجيل العملية بنجاح"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /////////////////////////////////// Create Bond /////////////////////////////////
        [HttpPost]
        public async Task<IActionResult> CreateFoodBond([FromBody] FoodBondCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var bondId = await _repository.CreateFoodBondAsync(request);
               
                var bond = await _repository.GetFoodBondByIdAsync(bondId);

                return CreatedAtAction(
                    nameof(GetFoodBond),
                    new { id = bondId },
                    new
                    {
                        BondId = bondId,
                        QRCode = bond.QRCode,
                        Message = "تم إنشاء السند بنجاح"
                    });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    Error = ex.Message,
                    Details = "تأكد من صحة اسم المستفيد والمطعم"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "خطأ داخلي",
                    Details = ex.Message
                });
            }
        }

        /////////////////////////////////// Get By Id /////////////////////////////////
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodBond(int id)
        {
            var foodBond = await _repository.GetFoodBondByIdAsync(id);
            return foodBond == null ? NotFound() : Ok(foodBond);
        }

        /////////////////////////////////// Get All /////////////////////////////////
        [HttpGet]
        public async Task<IActionResult> GetAllFoodBonds()
        {
            var bonds = await _repository.GetAllFoodBondsAsync();
            return Ok(bonds);
        }

        /////////////////////////////////// Restaurant Balance /////////////////////////////////
        [HttpGet("restaurant-balance/{restaurantId}")]
        public async Task<IActionResult> GetRestaurantBalance(int restaurantId)
        {
            try
            {
                var balance = await _repository.GetRestaurantBalance(restaurantId);

                return Ok(new
                {
                    RestaurantId = restaurantId,
                    Balance = balance
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}