using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Core.KafkaModels;

namespace TestSystem.Infrastructure.KafkaServices;

public class KafkaConsumer : BackgroundService
{
    private readonly string _topic;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> _callbacks;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger, ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> callbacks, IServiceScopeFactory scopeFactory)
    {
        _callbacks = callbacks;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _topic = configuration["Kafka:ResultTopic"] ?? "code_executor_result";
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "code-execution-group",
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
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka consumer started and subscribed to topic {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumerResult = _consumer.Consume(stoppingToken);
                    if (consumerResult == null) continue;
                    await ProcessMessage(consumerResult);
                    _consumer.Commit(consumerResult);
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Error occured while processing message, {Reason}", e.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer is stopping due to cancellation request.");
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessMessage(ConsumeResult<Ignore, string> message)
    {
        try
        {
            var result = JsonSerializer.Deserialize<CodeExecutionResult>(message.Message.Value);
            if (result != null && _callbacks.TryRemove(result.CorrelationId, out var tsc))
            {
                var packageRepository = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IPackageRepository>();
                var package = await packageRepository.GetByIdAsync(result.PackageId);
                if (result.FailedTests == 0)
                {
                    package.Status = PackageStatus.Accepted;
                }
                else
                {
                    package.Status = PackageStatus.Rejected;
                }
                await packageRepository.UpdateAsync(package);
                tsc.SetResult(result);
            }
            else
            {
                _logger.LogInformation("Received unknown correlation ID: {CorrelationId}", result?.CorrelationId);
            }
        }
        catch (JsonException e)
        {
            _logger.LogError("Failed to deserialize message: {Error}", e.Message);
        }
    }
}