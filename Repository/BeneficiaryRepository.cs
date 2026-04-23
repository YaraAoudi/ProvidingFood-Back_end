using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
	public class BeneficiaryRepository : IBeneficiaryRepository
	{

		private readonly string _connectionString;

		public BeneficiaryRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task<int> SubmitRequestAsync(BeneficiaryRequest request)
		{
			using var connection = new SqlConnection(_connectionString);

			var query = @"
INSERT INTO BeneficiaryRequests
(UserId, FullName, PhoneNumber, FamilySize, MaritalStatus,
 MaritalStatusProofImage, FamilySizeProofImage, Status)
VALUES
(@UserId, @FullName, @PhoneNumber, @FamilySize, @MaritalStatus,
 @MaritalStatusProofImage, @FamilySizeProofImage, 'Pending');

SELECT CAST(SCOPE_IDENTITY() as int);";

			return await connection.ExecuteScalarAsync<int>(query, request);
		}

		public async Task<BeneficiaryRequest?> GetPendingRequestAsync(int requestId)
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.QueryFirstOrDefaultAsync<BeneficiaryRequest>(
				"SELECT * FROM BeneficiaryRequests WHERE RequestId = @Id AND Status = 'Pending'",
				new { Id = requestId });
		}

		public async Task InsertBeneficiaryAsync(BeneficiaryRequest request)
		{
			using var connection = new SqlConnection(_connectionString);

			var query = @"
INSERT INTO Beneficiaries
(UserId, FullName, PhoneNumber, FamilySize,
 MaritalStatus, MaritalStatusProofImage, FamilySizeProofImage, IsActive)
VALUES
(@UserId, @FullName, @PhoneNumber, @FamilySize,
 @MaritalStatus, @MaritalStatusProofImage, @FamilySizeProofImage, 1);";

			await connection.ExecuteAsync(query, request);
		}

		public async Task UpdateRequestStatusAsync(int requestId, string status)
		{
			using var connection = new SqlConnection(_connectionString);

			await connection.ExecuteAsync(
				"UPDATE BeneficiaryRequests SET Status = @Status WHERE RequestId = @Id",
				new { Status = status, Id = requestId });
		}

		public async Task<int> RejectRequestAsync(int requestId)
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.ExecuteAsync(
				"UPDATE BeneficiaryRequests SET Status = 'Rejected' WHERE RequestId = @Id",
				new { Id = requestId });
		}

		public async Task<IEnumerable<BeneficiaryRequest>> GetPendingRequestsAsync()
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.QueryAsync<BeneficiaryRequest>(
				"SELECT * FROM BeneficiaryRequests WHERE Status = 'Pending' ORDER BY CreatedDate DESC");
		}

		public async Task<IEnumerable<BeneficiaryRequest>> GetAllRequestsAsync()
		{
			using var connection = new SqlConnection(_connectionString);

			return await connection.QueryAsync<BeneficiaryRequest>(
				"SELECT * FROM BeneficiaryRequests ORDER BY CreatedDate DESC");
		}
	


	public async Task<IEnumerable<Beneficiary>> GetAllBeneficiariesAsync()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				string query = "SELECT * FROM [Beneficiaries]";
				return await connection.QueryAsync<Beneficiary>(query);
			}
		}

		private string KeepOldIfEmpty(string newValue, string oldValue)
		{
			return string.IsNullOrWhiteSpace(newValue) ? oldValue : newValue;
		}
		public async Task<bool> UpdateBeneficiaryAsync(Beneficiary newBeneficiary)
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			using var transaction = connection.BeginTransaction();

			try
			{
				// جلب السجل القديم للتحقق أو للإبقاء على البيانات القديمة إن لم يتم تمرير الجديدة
				var existing = await connection.QuerySingleOrDefaultAsync<Beneficiary>(
					"SELECT * FROM Beneficiaries WHERE BeneficiaryId = @BeneficiaryId",
					new { newBeneficiary.BeneficiaryId }, transaction);

				if (existing == null)
					return false;

				// إبقاء القيم القديمة إذا لم تُمرر قيم جديدة
				newBeneficiary.FullName = KeepOldIfEmpty(newBeneficiary.FullName, existing.FullName);
				newBeneficiary.PhoneNumber = KeepOldIfEmpty(newBeneficiary.PhoneNumber, existing.PhoneNumber);
				newBeneficiary.FamilySize = newBeneficiary.FamilySize == 0 ? existing.FamilySize : newBeneficiary.FamilySize;
				newBeneficiary.IsActive = newBeneficiary.IsActive;

				
				string updateQuery = @"
UPDATE Beneficiaries SET
	FullName = @FullName,
	PhoneNumber = @PhoneNumber,
	FamilySize = @FamilySize,
	IsActive = @IsActive
WHERE BeneficiaryId = @BeneficiaryId";

				int affectedRows = await connection.ExecuteAsync(updateQuery, newBeneficiary, transaction);
				transaction.Commit();
				return affectedRows > 0;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public async Task<bool> DeleteBeneficiariesUserAsync(int BeneficiaryId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();


				using (var transaction = connection.BeginTransaction())
				{
					try
					{



						string deleteUserQuery = @"DELETE FROM [Beneficiaries] 
                                         WHERE BeneficiaryId = @BeneficiaryId";

						int BeneficiaryRows = await connection.ExecuteAsync(
							deleteUserQuery,
							new { BeneficiaryId = BeneficiaryId },
							transaction);


						transaction.Commit();


						return (BeneficiaryRows > 0);
					}
					catch
					{

						transaction.Rollback();
						throw;
					}
				}

			}

		}
        public async Task<IEnumerable<BeneficiaryRequest>> GetRequestsByUserEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
    SELECT br.*
    FROM BeneficiaryRequests br
    INNER JOIN [User] u ON br.UserId = u.UserId
    WHERE u.Email = @Email
    ORDER BY br.RequestId DESC";

            return await connection.QueryAsync<BeneficiaryRequest>(sql, new { Email = email });
        }

    }
}
