using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using System.Data.SqlClient;
using System.Security.Claims;

namespace ProvidingFood2.Repository
{
	public class DonationRestaurantRepository:IDonationRestaurantRepository
	{
		private readonly string _connectionString;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DonationRestaurantRepository(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection");

			

		}

		public async Task<IEnumerable<DonationRequestDto>> GetDonationsAsync()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				var sql = @"
               SELECT 
    d.Quantity, 
    d.DateDonated, 
    r.RestaurantName,
    de.Name AS DeliveryLocation
FROM 
    Donation d
INNER JOIN 
    Restaurant r ON d.RestaurantId = r.RestaurantId
INNER JOIN 
    Delivery de ON d.DeliveryId = de.DeliveryId
ORDER BY 
    d.DateDonated DESC";

				return await connection.QueryAsync<DonationRequestDto>(sql);
			}
		}


		public async Task<bool> AddDonationAsync(string restaurantName, int quantity, DateTime dateDonated, string deliveryLocation)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				// التحقق من وجود المطعم
				var restaurantId = await connection.QueryFirstOrDefaultAsync<int?>(
					"SELECT RestaurantId FROM Restaurant WHERE RestaurantName = @RestaurantName",
					new { RestaurantName = restaurantName });

				if (restaurantId == null)
					throw new KeyNotFoundException("المطعم غير موجود");

				// التحقق من وجود مكان التسليم
				var deliveryId = await connection.QueryFirstOrDefaultAsync<int?>(
					"SELECT DeliveryId FROM Delivery WHERE Name = @DeliveryLocation",
					new { DeliveryLocation = deliveryLocation });

				if (deliveryId == null)
					throw new KeyNotFoundException("مكان التسليم غير موجود");

				var sql = @"
        INSERT INTO Donation 
            (RestaurantId, Quantity, DateDonated, DeliveryId)
        VALUES 
            (@RestaurantId, @Quantity, @DateDonated, @DeliveryId)";

				var affectedRows = await connection.ExecuteAsync(sql, new
				{
					RestaurantId = restaurantId,
					Quantity = quantity,
					DateDonated = dateDonated,
					DeliveryId = deliveryId
				});

				return affectedRows > 0;
			}
		}

	}
}
