using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using ProvidingFood2.Service;
using System.Security.Claims;

namespace ProvidingFood2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _service;

		public UserController(IUserService service)
		{
			_service = service;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(UserDto request)
		{
			var result = await _service.RegisterAsync(request);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(Login login)
		{
			var result = await _service.LoginAsync(login);

			if (!result.Success)
				return Unauthorized(result);

			return Ok(result);
		}
	
	[Authorize(Roles = "Admin")]
		[HttpGet("admin/dashboard")]
		public IActionResult GetAdminDashboard()
		{
			return Ok("أهلاً بك في لوحة تحكم المشرف");
		}

		[Authorize(Roles = "Donor,Restaurant")]
		[HttpGet("user/profile")]
		public IActionResult GetUserProfile()
		{
			var role = User.FindFirst(ClaimTypes.Role)?.Value;
			return Ok($"أنت مستخدم من نوع: {role}");
		}

	}
}
