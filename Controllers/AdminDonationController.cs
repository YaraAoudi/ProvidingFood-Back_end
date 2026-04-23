using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Service;
using System.Data.SqlClient;

namespace ProvidingFood2.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminDonationController : ControllerBase
    {
        private readonly IAdminDonationService _service;
        private readonly string _connectionString;

        public AdminDonationController(IAdminDonationService service, IConfiguration config)
        {
            _service = service;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        

        [HttpGet("donations")]
        public async Task<IActionResult> GetAllDonations([FromQuery] string? region)
        {
            var data = await _service.GetAllDonationsAsync(region);
            return Ok(data);
        }

        [HttpPost("assign")]
        
        public async Task<IActionResult> Assign(int bondDonationId, int beneficiaryId, int count)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
    INSERT INTO BondAssignments
    (BondDonationId, BeneficiaryId, NumberOfBonds, AssignedAt)
    VALUES
    (@BondDonationId, @BeneficiaryId, @Count, GETUTCDATE())",
            new { bondDonationId, beneficiaryId, count });

            return Ok("تم التوزيع");
        }
    }
}
