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

public class KafkaConsumer : BackgroundService
{
    private readonly string _topic;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> _callbacks;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumer> _logger;

    public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger, ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> callbacks, IServiceScopeFactory scopeFactory)
    {
        _callbacks = callbacks;
        _scopeFactory = scopeFactory;
        _topic = configuration["Kafka:ResultTopic"] ?? "code_executor_result";
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "code-execution-group",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _logger = logger;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

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
                    _logger.LogError(e, "Error consuming message");
                }
            }
        }
        catch (OperationCanceledException)
        {
           _logger.LogInformation("Consumer loop canceled");
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
            if (result == null) return;

            _logger.LogInformation("Processing result for CorrelationId: {CorrId}", result.CorrelationId);

            using (var scope = _scopeFactory.CreateScope())
            {
                var packageRepository = scope.ServiceProvider.GetRequiredService<IPackageRepository>();
                var package = await packageRepository.GetByIdAsync(result.PackageId);

                if (package != null)
                {
                    package.Status = result.FailedTests == 0 ? PackageStatus.Accepted : PackageStatus.Rejected;
                    await packageRepository.UpdateAsync(package);
                    _logger.LogInformation("Package {Id} updated in DB to {Status}", package.Id, package.Status);
                }
                else
                {
                    _logger.LogWarning("Package {Id} not found in database", result.PackageId);
                }

                if (_callbacks.TryRemove(result.CorrelationId, out var tsc))
                {
                    tsc.SetResult(result);
                }
                else
                {
                    _logger.LogWarning("CorrelationId {Id} not found in active callbacks", result.CorrelationId);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in ProcessMessage");
        }
    }
}