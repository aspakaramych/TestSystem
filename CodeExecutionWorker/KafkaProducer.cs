using System.Text.Json;
using CodeExecutionWorker.Models;
using Confluent.Kafka;

namespace CodeExecutionWorker;

public class KafkaProducer : IHostedService, IDisposable
{
    private IProducer<Null, string>? _producer;
    private readonly string _topic;
    private readonly string _bootstrapServices; 
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _topic = configuration["Kafka:ResultTopic"] ?? "code_executor_result";
        _bootstrapServices = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _bootstrapServices,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
        _logger.LogInformation("Kafka producer started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _logger.LogInformation("Kafka producer stopped");
        return Task.CompletedTask;
    }
    
    public async Task ProduceAsync(CodeExecutionResult message)
    {
        if (_producer == null) throw new InvalidOperationException("Producer is not initialized");
        var json = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}