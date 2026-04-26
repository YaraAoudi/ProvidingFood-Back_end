using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

public class VoucherRepository : IVoucherRepository
{
	private readonly string _connectionString;

	public VoucherRepository(IConfiguration configuration)
	{
		_connectionString = configuration.GetConnectionString("DefaultConnection");
	}

	public async Task<int> CreateAsync(Voucher voucher)
	{
		using var connection = new SqlConnection(_connectionString);

		return await connection.ExecuteScalarAsync<int>(@"
INSERT INTO Voucher
(BeneficiaryId, BeneficiaryName, StoreName, StoreLocation,
 BasketCount, ExpiryDate, QRCode, Status, CreatedAt)
VALUES
(@BeneficiaryId, @BeneficiaryName, @StoreName, @StoreLocation,
 @BasketCount, @ExpiryDate, @QRCode, @Status, @CreatedAt);

SELECT CAST(SCOPE_IDENTITY() as int);", voucher);


	}

	public async Task<Voucher?> GetByCodeAsync(string code)
	{
		using var connection = new SqlConnection(_connectionString);

		return await connection.QueryFirstOrDefaultAsync<Voucher>(
			"SELECT * FROM Voucher WHERE QRCode = @Code",
			new { Code = code });
	}

    public async Task<Voucher?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Voucher>(
            @"
        SELECT 
            VoucherId,
            BeneficiaryId,
            BeneficiaryName,
            StoreName,
            StoreLocation,
            BasketCount,
            ExpiryDate,
            QRCode,
            CreatedAt,
            CASE 
                WHEN ExpiryDate < GETUTCDATE() THEN 'Expired'
                ELSE 'Active'
            END AS Status
        FROM Voucher
        WHERE VoucherId = @Id",
            new { Id = id });
    }
    public async Task MarkAsUsedAsync(int voucherId)
	{
		using var connection = new SqlConnection(_connectionString);

		await connection.ExecuteAsync(@"
UPDATE Voucher
SET Status = 'Used',
    UsedAt = GETUTCDATE()
WHERE VoucherId = @VoucherId
AND Status = 'Active'",
		new { VoucherId = voucherId });
	}
    public async Task<IEnumerable<Voucher>> GetVoucherAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = @"
        SELECT 
            VoucherId,
            BeneficiaryId,
            BeneficiaryName,
            StoreName,
            StoreLocation,
            BasketCount,
            ExpiryDate,
            QRCode,
            CreatedAt,
            CASE 
                WHEN ExpiryDate < GETUTCDATE() THEN 'Expired'
                ELSE 'Active'
            END AS Status
        FROM Voucher";

            return await connection.QueryAsync<Voucher>(query);
        }


    }

    public async Task<int?> GetUserIdByBeneficiaryId(int beneficiaryId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int?>(
            "SELECT UserId FROM Beneficiaries WHERE BeneficiaryId = @Id",
            new { Id = beneficiaryId }
        );
    }


}