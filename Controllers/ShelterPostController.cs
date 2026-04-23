using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using System.Security.Claims;

[ApiController]
[Route("api/shelter-posts")]
public class ShelterPostController : ControllerBase
{
    private readonly ShelterPostService _service;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ShelterPostController(ShelterPostService service, IHttpContextAccessor accessor)
    {
        _service = service;
        _httpContextAccessor = accessor;
    }

    [HttpPost]
    [Authorize(Roles = "Shelter Owner")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateShelterPostDto dto)
    {
        // 🧠 أخذ UserId من التوكن
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

        if (userIdClaim == null)
            throw new Exception("UserId not found in token");

        var userId = int.Parse(userIdClaim.Value);

        var postId = await _service.CreatePostAsync(dto, userId);

        return Ok(new
        {
            success = true,
            postId
        });
    }
}