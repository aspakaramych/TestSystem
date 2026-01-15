using CodeExecutionWorker.Models;
using Confluent.Kafka;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CodeExecutionWorker;

public class CodeExecutionWorker : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaProducer _producer;
    private readonly DockerService _dockerService;
    private readonly string _requestTopic;

    public CodeExecutionWorker(IConfiguration configuration, KafkaProducer producer, DockerService dockerService, ILogger<CodeExecutionWorker> logger)
    {
        _producer = producer;
        _dockerService = dockerService;
        _requestTopic = configuration["Kafka:RequestTopic"] ?? "code_executor_request";
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "docker-executor-worker-group",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_requestTopic);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult == null) continue;

                    await ProcessTaskAsync(consumeResult.Message.Value);

                    _consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
        catch (Exception e)
        {
            
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessTaskAsync(string json)
    {
        var request = JsonSerializer.Deserialize<CodeExecutionRequest>(json);
        if (request == null) return;
        var result = await _dockerService.ExecuteCodeAsync(request);
        await _producer.ProduceAsync(result);   
    }
}