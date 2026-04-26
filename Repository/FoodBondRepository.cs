using Dapper;
using ProvidingFood2.DTO;
using ProvidingFood2.Service;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class FoodBondRepository : IFoodBondRepository
	{
		private readonly string _connectionString;
        private readonly INotificationService _notificationService;
        public FoodBondRepository(string connectionString , INotificationService notificationService)
		{
			_connectionString = connectionString;
            _notificationService = notificationService;
		}


		///////////////////////////////////Function for Scan QRCode/////////////////////
		public async Task<QRScanResult> ScanQRCodeAsync(string qrCode)
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
    SELECT 
        b.BondId,
        b.NumberOfMeals,
        b.ExpiryDate,
        s.StatusName AS Status,
        ben.FullName AS BeneficiaryName,
        r.RestaurantName AS RestaurantName
    FROM FoodBonds b
    JOIN FoodBondStatus s ON b.StatusId = s.StatusId
    JOIN Beneficiaries ben ON b.BeneficiaryId = ben.BeneficiaryId
    JOIN Restaurant r ON b.RestaurantId = r.RestaurantId
    WHERE b.QRCode = @QRCode";

			var result = await connection.QueryFirstOrDefaultAsync<QRScanResult>(sql, new { QRCode = qrCode });

			if (result == null)
				throw new KeyNotFoundException("QR Code غير صالح");

			return result;
		}
		///////////////////////////////////Function for Change bonds status/////////////////////
		public async Task<bool> UpdateBondStatusAsync(int bondId, string newStatus)
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
                UPDATE FoodBonds
                SET StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = @Status)
                WHERE BondId = @BondId";

			var affected = await connection.ExecuteAsync(sql,
				new { BondId = bondId, Status = newStatus });

			return affected > 0;
		}
		///////////////////////////////////Function for Validate expir of Bond/////////////////////
		public async Task CheckAndExpireBondsAsync()
		{
			using var connection = new SqlConnection(_connectionString);

			const string sql = @"
                UPDATE FoodBonds
                SET StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = 'Expired')
                WHERE StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = 'Pending')
                AND ExpiryDate < GETUTCDATE()";

			await connection.ExecuteAsync(sql);
		}
        ///////////////////////////////////Function for Screate Food Bond/////////////////////
        public async Task<int> CreateFoodBondAsync(FoodBondCreateRequest request)
        {
            await using var connection = new SqlConnection(_connectionString);

            var amount = await connection.ExecuteScalarAsync<decimal>(
                "SELECT Price FROM BondSettings");

            var beneficiaryId = await connection.ExecuteScalarAsync<int?>(
                "SELECT BeneficiaryId FROM Beneficiaries WHERE FullName = @Name",
                new { Name = request.BeneficiaryName });

            var restaurantId = await connection.ExecuteScalarAsync<int?>(
                "SELECT RestaurantId FROM Restaurant WHERE RestaurantName = @Name",
                new { Name = request.RestaurantName });

            var qrCode = $"FBOND_{Guid.NewGuid()}";

            var bondId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO FoodBonds 
        (BeneficiaryId, RestaurantId, StatusId, QRCode, NumberOfMeals, ExpiryDate, Amount)
        OUTPUT INSERTED.BondId
        VALUES (@BeneficiaryId, @RestaurantId, 1, @QRCode, @NumberOfMeals, @ExpiryDate, @Amount)",
                new
                {
                    BeneficiaryId = beneficiaryId,
                    RestaurantId = restaurantId,
                    QRCode = qrCode,
                    request.NumberOfMeals,
                    request.ExpiryDate,
                    Amount = amount
                });

            // 🔥🔥🔥 هون الإضافة
            var userId = await connection.ExecuteScalarAsync<int?>(
                "SELECT UserId FROM Beneficiaries WHERE BeneficiaryId = @Id",
                new { Id = beneficiaryId });

            if (userId != null)
            {
                await _notificationService.SendNotificationAsync(
                    userId.Value,
                    "🎁 تم استلام سند طعام",
                    "تم إضافة سند جديد إلى حسابك"
                );
            }

            return bondId;
        }
        ///////////////////////////////////Function for get all Food Bond/////////////////////
        public async Task<FoodBondResponse?> GetFoodBondByIdAsync(int id)
		{
			await using var connection = new SqlConnection(_connectionString);

			var sql = @"
            SELECT 
    fb.BondId AS Id,
    b.FullName AS BeneficiaryName,
    r.RestaurantName AS RestaurantName,
    fb.NumberOfMeals,
    fb.CreatedAt,
    fb.QRCode,
    fb.ExpiryDate,
    s.StatusName
FROM FoodBonds fb
INNER JOIN Beneficiaries b ON fb.BeneficiaryId = b.BeneficiaryId
INNER JOIN Restaurant r ON fb.RestaurantId = r.RestaurantId
INNER JOIN FoodBondStatus s ON fb.StatusId = s.StatusId
WHERE fb.BondId = @Id;
";

			var result = await connection.QuerySingleOrDefaultAsync<FoodBondResponse>(sql, new { Id = id });

			return result;
		}
		public async Task<IEnumerable<FoodBondResponse>> GetAllFoodBondsAsync()
		{
			await using var connection = new SqlConnection(_connectionString);

			var sql = @"
		SELECT 
			fb.BondId AS Id,
			b.FullName AS BeneficiaryName,
			r.RestaurantName AS RestaurantName,
			fb.NumberOfMeals,
			fb.CreatedAt,
			fb.QRCode,
			fb.ExpiryDate,
			s.StatusName
		FROM FoodBonds fb
		INNER JOIN Beneficiaries b ON fb.BeneficiaryId = b.BeneficiaryId
		INNER JOIN Restaurant r ON fb.RestaurantId = r.RestaurantId
		INNER JOIN FoodBondStatus s ON fb.StatusId = s.StatusId;
	";

			var results = await connection.QueryAsync<FoodBondResponse>(sql);

			return results;
		}

        public async Task ConfirmBondReceiptAsync(int bondId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            var bond = await connection.QueryFirstAsync<dynamic>(
                @"SELECT BondId, RestaurantId, Amount, StatusId 
          FROM FoodBonds WHERE BondId = @Id",
                new { Id = bondId }, transaction);

            var status = await connection.QueryFirstAsync<string>(
                @"SELECT StatusName FROM FoodBondStatus WHERE StatusId = @Id",
                new { Id = bond.StatusId }, transaction);

            if (status == "Received")
                throw new Exception("مستخدم مسبقاً");

            // 🔹 تحديث الحالة
            await connection.ExecuteAsync(@"
        UPDATE FoodBonds
        SET StatusId = (SELECT StatusId FROM FoodBondStatus WHERE StatusName = 'Received')
        WHERE BondId = @Id",
                new { Id = bondId }, transaction);

            // 🔹 تسجيل Credit
            await connection.ExecuteAsync(@"
        INSERT INTO RestaurantTransactions
        (RestaurantId, BondId, Amount, Type)
        VALUES (@RestaurantId, @BondId, @Amount, 'Credit')",
                new
                {
                    RestaurantId = bond.RestaurantId,
                    BondId = bondId,
                    Amount = bond.Amount
                }, transaction);

            transaction.Commit();
        }
        public async Task<decimal> GetRestaurantBalance(int restaurantId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<decimal>(@"
        SELECT ISNULL(SUM(CASE 
            WHEN Type = 'Credit' THEN Amount 
            ELSE -Amount END),0)
        FROM RestaurantTransactions
        WHERE RestaurantId = @RestaurantId",
                new { RestaurantId = restaurantId });
        }
    }
}

