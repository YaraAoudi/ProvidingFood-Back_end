using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Service;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class BeneficiaryController : ControllerBase
{
	private readonly IBeneficiaryService _service;

	public BeneficiaryController(IBeneficiaryService service)
	{
		_service = service;
	}

	
	[Authorize(Roles = "Beneficiary")]
	[HttpPost("submit")]
    public async Task<IActionResult> Submit([FromForm] BeneficiaryRequestDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.SubmitRequestAsync(dto, userId);
        return Ok(result);
    }


    [Authorize(Roles = "Admin")]
	[HttpGet("pending")]
	public async Task<IActionResult> GetPendingRequests()
	{
		var requests = await _service.GetPendingRequestsAsync();
		return Ok(requests);
	}

	[Authorize(Roles = "Admin")]
	[HttpGet("all")]
	public async Task<IActionResult> GetAllRequests()
	{
		var requests = await _service.GetAllRequestsAsync();
		return Ok(requests);
	}

	
	[Authorize(Roles = "Admin")]
	[HttpPost("approve/{id}")]
	public async Task<IActionResult> Approve(int id)
	{
		var result = await _service.ApproveRequestAsync(id);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	
	[Authorize(Roles = "Admin")]
	[HttpPost("reject/{id}")]
	public async Task<IActionResult> Reject(int id)
	{
		var result = await _service.RejectRequestAsync(id);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

    [HttpGet("my-requests")]
    [Authorize(Roles = "Beneficiary")]
    public async Task<IActionResult> GetMyRequests()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return Unauthorized("Email not found in token");

        var result = await _service.GetRequestsByUserEmailAsync(email);

        return Ok(result);
    }

}
