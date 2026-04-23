using Dapper;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class DonationIndividalRepository : IDonationIndividalRepository
	{
		private readonly string _connectionString;

		public DonationIndividalRepository(string connectionString)
		{
			_connectionString = connectionString;
		}
		// Create Request by Donor
		public async Task<bool> AddDonationAsync(DonorDto donorDto)
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			var userId = await connection.QueryFirstOrDefaultAsync<int?>(
				"SELECT UserId FROM [User] WHERE Email = @Email",
				new { Email = donorDto.UserEmail });

			if (userId == null)
				throw new KeyNotFoundException("المستخدم غير موجود بالبريد الإلكتروني المحدد.");

			var requesId = await connection.QueryFirstOrDefaultAsync<int?>(
				"SELECT TOP 1 RequesId FROM Donation_Request_Status WHERE Name = @Name AND UserId = @UserId",
				new { Name = "Pending", UserId = userId });

			if (requesId == null)
			{
				requesId = await connection.ExecuteScalarAsync<int>(@"
                INSERT INTO Donation_Request_Status (Name, UserId)
                VALUES (@Name, @UserId);
                SELECT SCOPE_IDENTITY();",
					new { Name = "Pending", UserId = userId });
			}

			var sql = @"
            INSERT INTO DonationRequest 
                (FoodName, UserId, RequesId, UserType, Description, Image, Country, Vegetarian)
            VALUES
                (@FoodName, @UserId, @RequesId, @UserType, @Description, @Image, @Country, @Vegetarian)";

			var affected = await connection.ExecuteAsync(sql, new
			{
				donorDto.FoodName,
				UserId = userId,
				RequesId = requesId,
				donorDto.UserType,
				donorDto.Description,
				donorDto.Image,
				donorDto.Country,
				donorDto.Vegetarian
			});

			return affected > 0;
		}
		// Function for get the statu of request
		public async Task<IEnumerable<DonorWithStatusDto>> GetDonationsByUserEmailAsync(string userEmail)
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			var sql = @"
            SELECT dr.FoodName, dr.Description, dr.Country, dr.Vegetarian, d.Name AS Status
            FROM DonationRequest dr
            INNER JOIN [User] u ON dr.UserId = u.UserId
            INNER JOIN Donation_Request_Status d ON dr.RequesId = d.RequesId
            WHERE u.Email = @Email";

			return await connection.QueryAsync<DonorWithStatusDto>(sql, new { Email = userEmail });
		}
		//Function for update statue to each request by admin
		public async Task<bool> UpdateDonationStatusAsync(int requesId, string newStatus)
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			var sql = @"
        UPDATE Donation_Request_Status 
        SET Name = @NewStatus
        WHERE RequesId = @RequesId";

			var affected = await connection.ExecuteAsync(sql, new { NewStatus = newStatus, RequesId = requesId });

			return affected > 0;
		}

		public async Task<IEnumerable<DonorWithStatusDto>> GetAllDonationsAsync()
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			var sql = @"
        SELECT dr.FoodId , dr.RequesId ,dr.FoodName, dr.Description, dr.Country, dr.Vegetarian, d.Name AS Status 
        FROM DonationRequest dr
        INNER JOIN [User] u ON dr.UserId = u.UserId
        INNER JOIN Donation_Request_Status d ON dr.RequesId = d.RequesId";

			return await connection.QueryAsync<DonorWithStatusDto>(sql);
		}


	}
}
