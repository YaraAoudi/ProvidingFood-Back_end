using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DonationRestaurantController : ControllerBase

	{
		private readonly IDonationRestaurantRepository _donationResturant;

		public DonationRestaurantController(IDonationRestaurantRepository donationResturant)
		{
			_donationResturant = donationResturant;
		}

		[HttpGet]
		public async Task<IActionResult> GetDonations()
		{
			try
			{
				var donations = await _donationResturant.GetDonationsAsync();
				return Ok(donations);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> AddDonation([FromBody] DonationRequestDto request)
		{
			try
			{
				var success = await _donationResturant.AddDonationAsync(
					request.RestaurantName,
					request.Quantity,
					request.DateDonated,
					request.DeliveryLocation);

				if (success)
					return Ok(new { message = "تمت إضافة التبرع بنجاح" });
				else
					return BadRequest("فشل في إضافة التبرع");
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "حدث خطأ أثناء المعالجة", details = ex.Message });
			}

		}



	}
}

