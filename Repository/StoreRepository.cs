using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class StoreRepository : IStoreRepository
	{
		private readonly string _connectionString;

		public StoreRepository(IConfiguration config)
		{
			_connectionString = config.GetConnectionString("DefaultConnection");
		}

		public async Task<int> CreateRequestAsync(StoreDonationRequest request)
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.ExecuteScalarAsync<int>(@"
INSERT INTO StoreDonationRequests
(StoreUserId, StoreName, StoreLocation, PhoneNumber, BasketCount, BasketContents, Status, CreatedAt)
VALUES
(@StoreUserId, @StoreName, @StoreLocation, @PhoneNumber, @BasketCount, @BasketContents, @Status, @CreatedAt);

SELECT CAST(SCOPE_IDENTITY() as int);", request);
		}

		public async Task<IEnumerable<StoreDonationRequest>> GetByStoreUserIdAsync(int storeUserId)
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.QueryAsync<StoreDonationRequest>(
				"SELECT * FROM StoreDonationRequests WHERE StoreUserId = @StoreUserId",
				new { StoreUserId = storeUserId });
		}

		// ✅ عرض الطلبات المعلقة
		public async Task<IEnumerable<StoreDonationRequest>> GetPendingAsync()
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.QueryAsync<StoreDonationRequest>(
				"SELECT * FROM StoreDonationRequests WHERE Status = 'Pending'");
		}

        public async Task<IEnumerable<StoreDonationRequest>> GetAllRequestAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<StoreDonationRequest>(
                "SELECT * FROM StoreDonationRequests ");
        }

        // لجلب طلب محدد
        public async Task<StoreDonationRequest?> GetByIdAsync(int id)
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.QueryFirstOrDefaultAsync<StoreDonationRequest>(
				"SELECT * FROM StoreDonationRequests WHERE RequestId = @Id",
				new { Id = id });
		}

		// ✅ موافقة الأدمن
		public async Task ApproveAsync(int id)
		{
			using var connection = new SqlConnection(_connectionString);

			await connection.ExecuteAsync(@"
UPDATE StoreDonationRequests
SET Status = 'Approved',
    ApprovedAt = GETUTCDATE()
WHERE RequestId = @Id
AND Status = 'Pending'",
			new { Id = id });
		}

        public async Task RejectAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
UPDATE StoreDonationRequests
SET Status = 'Rejected'
WHERE RequestId = @Id
AND Status = 'Pending'",
            new { Id = id });
        }
    }

}
