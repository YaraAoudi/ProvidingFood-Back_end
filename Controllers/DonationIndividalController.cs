using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DonationIndividalController : ControllerBase
	{
		private readonly IDonationIndividalRepository _donorRepository;

		public DonationIndividalController(IDonationIndividalRepository donorRepository)
		{
			_donorRepository = donorRepository;
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddDonation([FromBody] DonorDto dto)
		{
			try
			{
				var result = await _donorRepository.AddDonationAsync(dto);
				if (result)
					return Ok(new { message = "تمت إضافة التبرع بنجاح" });
				return StatusCode(500, new { message = "فشل في إضافة التبرع" });
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "خطأ في الخادم", details = ex.Message });
			}
		}

		[HttpGet("my-donations")]
		public async Task<IActionResult> GetMyDonations([FromQuery] string email)
		{
			try
			{
				var data = await _donorRepository.GetDonationsByUserEmailAsync(email);
				return Ok(data);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "خطأ في جلب البيانات", details = ex.Message });
			}
		}
		[HttpPut("update-status")]
		public async Task<IActionResult> UpdateStatus([FromQuery] int requesId, [FromQuery] string status)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(status) || !new[] { "Pending", "Approved", "Rejected" }.Contains(status))
				{
					return BadRequest(new { message = "حالة غير صالحة. يجب أن تكون Pending أو Approved أو Rejected" });
				}

				var updated = await _donorRepository.UpdateDonationStatusAsync(requesId, status);
				if (updated)
					return Ok(new { message = "تم تحديث الحالة بنجاح" });

				return NotFound(new { message = "لم يتم العثور على الطلب" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "حدث خطأ", details = ex.Message });
			}
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAllDonations()
		{
			try
			{
				var data = await _donorRepository.GetAllDonationsAsync();
				return Ok(data);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "حدث خطأ أثناء جلب الطلبات", details = ex.Message });
			}
		}

	}
}
