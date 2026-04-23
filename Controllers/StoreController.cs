using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Model;
using ProvidingFood2.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
	[Route("api/store")]
	[ApiController]
	
	public class StoreController : ControllerBase
	{
		private readonly IStoreService _service;

		public StoreController(IStoreService service)
		{
			_service = service;
		}

		[Authorize]
		[Authorize(Roles = "Store Owner")]
		[HttpPost("submit")]
		public async Task<IActionResult> Submit(StoreDonationRequest request)
		{
			try
			{
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

				if (userIdClaim == null)
					return BadRequest("User ID claim not found in JWT");

				int storeUserId = int.Parse(userIdClaim.Value);

				var id = await _service.SubmitRequestAsync(request, storeUserId);

				return Ok(new
				{
					Message = "تم إرسال الطلب بنجاح",
					RequestId = id
				});
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


		// عرض طلباتي
		[Authorize]
		[Authorize(Roles = "Store Owner")]
		[HttpGet("my-requests")]
		public async Task<IActionResult> MyRequests()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

			if (userIdClaim == null)
				return BadRequest("User ID claim not found in JWT");

			int storeUserId = int.Parse(userIdClaim.Value);

			var requests = await _service.GetMyRequestsAsync(storeUserId);

			return Ok(requests);
		}


		// عرض الطلبات المعلقة
		[Authorize(Roles = "Admin")]
		[HttpGet("pending")]
		public async Task<IActionResult> GetPending()
		{
			var requests = await _service.GetPendingRequestsAsync();
			return Ok(requests);
		}

        [Authorize(Roles = "Admin")]
        [HttpGet("allRequest")]
        public async Task<IActionResult> GetAllRequest()
        {
            var requests = await _service.GetRequestsAsync();
            return Ok(requests);
        }

        // الموافقة على طلب
        [Authorize(Roles = "Admin")]
		[HttpPost("approve/{id}")]
		public async Task<IActionResult> Approve(int id)
		{
			try
			{
				await _service.ApproveRequestAsync(id);
				return Ok("تمت الموافقة على الطلب");
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
        [Authorize(Roles = "Admin")]
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            await _service.RejectAsync(id);
            return Ok(new { message = "Request rejected successfully" });
        }
    }
}