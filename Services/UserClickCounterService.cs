using System.Text.Json;
using Confluent.Kafka;
using StackExchange.Redis;
using WebAPIProgram.Models.Kafka;

namespace WebAPIProgram.Services;

public class UserClickCounterService : BackgroundService
{
    private readonly ILogger<UserClickCounterService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;

    public UserClickCounterService(ILogger<UserClickCounterService> logger, IServiceProvider serviceProvider, IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _redis = redis;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        
        _logger.LogInformation("Starting Profile click service");
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = "profile-view-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe("profile-views");

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Entered while loop of the servoce");
            try
            {
                _logger.LogInformation("huh");

                var cr = consumer.Consume(TimeSpan.FromMilliseconds(100));
                if (cr == null) continue; // No message received
                _logger.LogInformation("kur");

                var viewEvent = JsonSerializer.Deserialize<ProfileViewEvent>(cr.Message.Value);
                
                _logger.LogInformation("Checking if Event is null");
                
                if (viewEvent == null) continue;
                
                _logger.LogInformation("Event is not null");

                var db = _redis.GetDatabase();
                var redisKey = $"profileview:{viewEvent.ViewerId}:{viewEvent.ViewedUserId}";

                if (await db.KeyExistsAsync(redisKey)) continue;

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var user = await context.Artists.FindAsync(viewEvent.ViewedUserId);
                if (user != null)
                {
                    user.ProfileViews += 1;
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Incremented Artist {viewedUser}'s profile view count. Viewed by User {viewerUser}", viewEvent.ViewedUserId, viewEvent.ViewerId) ;
                }

                await db.StringSetAsync(redisKey, "1", TimeSpan.FromMinutes(10));
            }
            catch (ConsumeException ex)
            {
                _logger.LogError("Kafka consume error: {error}", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError("Processing error: {error}", ex.Message);

            }
        }

        consumer.Close();
    }
}