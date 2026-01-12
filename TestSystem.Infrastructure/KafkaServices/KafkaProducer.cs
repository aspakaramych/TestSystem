using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestSystem.Infrastructure.KafkaServices;

public class KafkaProducer : IHostedService, IDisposable
{
    private IProducer<Null, string>? _producer;
    private readonly string _topic;
    private readonly string _bootstrapServices;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _bootstrapServices = configuration["Kafka:BootstrapServers"] ?? "localhost:9082";
        _topic = configuration["Kafka:RequestTopic"] ?? "code_executor_request";
        _logger = logger;
    }
    
    public void Dispose()
    {
        _producer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServices,
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 100,
        };
        
        _producer = new ProducerBuilder<Null, string>(config).Build();
        _logger.LogInformation("Kafka producer started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Kafka producer stopped");
        _producer?.Flush(TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    public async Task ProduceAsync<T>(string? topic, T message)
    {
        if (_producer == null)
        {
            _logger.LogError("Kafka Producer is not init");
            throw new InvalidOperationException("Kafka Producer is not init");
        }

        try
        {
            var targetTopic = topic ?? _topic;
            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string>
            {
                Value = json,
            };
            var result = await _producer.ProduceAsync(targetTopic, kafkaMessage);
            _logger.LogInformation($"Produced message to topic: {result.TopicPartitionOffset}");
        }
        catch (ProduceException<Null, string> e)
        {
            _logger.LogError(e.ToString());
        }
    }
}