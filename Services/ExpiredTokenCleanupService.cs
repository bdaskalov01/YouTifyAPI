using WebAPIProgram.Util;

namespace WebAPIProgram.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class ExpiredTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(AuthConstants.thirtyMinutes);

    public ExpiredTokenCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await context.RefreshTokens
                    .Where(t => t.ExpiryTime < DateTime.UtcNow)
                    .ExecuteDeleteAsync();
                
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
