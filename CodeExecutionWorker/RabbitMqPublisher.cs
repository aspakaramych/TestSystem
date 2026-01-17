using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace CodeExecutionWorker;

public class RabbitMqPublisher : IHostedService, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _hostname = configuration["RabbitMQ:HostName"] ?? "rabbitmq";
        _queueName = configuration["RabbitMQ:ResultQueue"] ?? "code_execution_results";
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ Publisher started. Host: {Host}, Queue: {Queue}", _hostname, _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Could not start RabbitMQ Publisher");
            throw;
        }
    }

    public async Task ProduceAsync(CodeExecutionResult message)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogError("RabbitMQ channel is not open. Cannot publish result.");
            throw new InvalidOperationException("Channel is closed");
        }

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queueName,
            mandatory: true,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published execution result for CorrelationId: {CorrId}", message.CorrelationId);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ Publisher...");
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is { IsOpen: true }) await _channel.CloseAsync();
        if (_connection is { IsOpen: true }) await _connection.CloseAsync();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}