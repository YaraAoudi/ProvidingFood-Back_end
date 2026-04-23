
namespace ProvidingFood2.Repository
{
	public class BondStatusBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<BondStatusBackgroundService> _logger;

		public BondStatusBackgroundService(
			IServiceProvider services,
			ILogger<BondStatusBackgroundService> logger)
		{
			_services = services;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Checking for expired bonds...");

				using (var scope = _services.CreateScope())
				{
					var repo = scope.ServiceProvider.GetRequiredService<IFoodBondRepository>();
					await repo.CheckAndExpireBondsAsync();
				}

				await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // التحقق كل ساعة
			}
		}
	}

}

