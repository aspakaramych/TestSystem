using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Core.RabbitModels;

namespace TestSystem.Infrastructure.RabbitMqService;

public class RabbitMqConsumer : IHostedService, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> _callbacks;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public RabbitMqConsumer(
        IConfiguration configuration,
        ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>> callbacks, 
        ILogger<RabbitMqConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _hostname = configuration["RabbitMQ:HostName"] ?? "rabbitmq";
        _queueName = configuration["RabbitMQ:ResultQueue"] ?? "code_execution_results";
        _callbacks = callbacks;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
         
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        await _channel.QueueDeclareAsync(queue: _queueName,
            durable: true, 
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var eventConsumer = new AsyncEventingBasicConsumer(_channel);
        
        eventConsumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var result = JsonSerializer.Deserialize<CodeExecutionResult>(message);
                if (result == null) throw new JsonException("Deserialization returned null");

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
                }
                
                if (_callbacks.TryRemove(result.CorrelationId, out var tsc))
                {
                    tsc.SetResult(result);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: eventConsumer);
        _logger.LogInformation("RabbitMQConsumer started on {Host}, queue {Queue}", _hostname, _queueName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel is { IsOpen: true }) await _channel.CloseAsync();
        if (_connection is { IsOpen: true }) await _connection.CloseAsync();
    }
}