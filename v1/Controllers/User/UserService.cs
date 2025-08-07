using System.Text.Json;
using Confluent.Kafka;
using StackExchange.Redis;
using WebAPIProgram.Models.Kafka;

namespace WebAPIProgram.Controllers;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _reddis;
    private readonly IProducer<Null, string> _producer;

    public UserService(IUserRepository userRepository, ApplicationDbContext dbContext, ILogger<UserService> logger, IConfiguration configuration, IConnectionMultiplexer reddis)
    {
        _userRepository = userRepository;
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        _reddis = reddis;
        
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };
        
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task getUserIdTest(string viewedUserId, string viewerId)
    {
        
        _logger.LogInformation("Serializing ProfileViewEvent");
        var message = JsonSerializer.Serialize(new ProfileViewEvent
        {
            ViewedUserId = viewedUserId,
            ViewerId = viewerId,
            Timestamp = DateTime.Now
        });

        var deliveryResult = await _producer.ProduceAsync("profile-views", new Message<Null, string> { Value = message });
        _logger.LogInformation($"Produced message to {deliveryResult.TopicPartitionOffset}");


    }
}