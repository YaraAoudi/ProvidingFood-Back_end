using Dapper;
using System.Data.SqlClient;
using ProvidingFood2.DTO;
using System.Drawing;


namespace ProvidingFood2.Repository
{
    public class AdminDonationRepository : IAdminDonationRepository
    {
        private readonly string _connectionString;

        public AdminDonationRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<DonationAdminDto>> GetAllDonationsAsync(string? region)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
    SELECT 
        u.fullName AS FullName,
        u.email AS Email,
        u.phoneNumber AS PhoneNumber,
        c.Amount,
        c.RegionName,
        c.Status,
        c.CreatedAt
    FROM CashDonations c 
    INNER JOIN [user] u ON c.DonorUserId = u.UserId
    WHERE c.Status = 'Paid'
    ORDER BY c.CreatedAt DESC";

            var builder = new SqlBuilder();

            if (!string.IsNullOrEmpty(region))
            {
                builder.Where("LOWER(c.RegionName) LIKE LOWER(N'%' + @Region + '%')");
            }

            var template = builder.AddTemplate(sql, new { Region = region });

            return await connection.QueryAsync<DonationAdminDto>(template.RawSql, template.Parameters);
        }

      
    }
}
