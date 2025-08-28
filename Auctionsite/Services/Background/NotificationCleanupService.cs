namespace Auctionsite.Services.Background
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using Auctionsite.Data;

    // This background service runs every 24 hours and deletes notifications older than 7 days from the database.
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run cleanup once every 24 hours

        public NotificationCleanupService(IServiceProvider services, ILogger<NotificationCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        // Called when the background service starts. The stoppingToken is used to gracefully handle cancellation when the app shuts down.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationCleanupService started.");

            // Continue running until the service is cancelled
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new service scope to avoid reusing a DbContext across executions
                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var cutoffDate = DateTime.Now.AddDays(-7); // Notifications older than 7 days will be deleted
                        var oldNotices = db.Notifications.Where(n => n.CreatedAt < cutoffDate);

                        int count = await oldNotices.CountAsync(stoppingToken);

                        if (count > 0)
                        {
                            db.Notifications.RemoveRange(oldNotices);
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Deleted {Count} old notifications.", count);
                        }
                        else
                        {
                            _logger.LogInformation("No old notifications found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while cleaning up notifications.");
                }

                // Wait for the next run, unless the service is stopping
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("NotificationCleanupService stopping.");
        }
    }


}
