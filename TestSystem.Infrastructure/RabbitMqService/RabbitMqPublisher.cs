using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TestSystem.Infrastructure.RabbitMqService;

public class RabbitMqPublisher : IHostedService, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _hostname;
    private readonly string _defaultQueue;
    private readonly ILogger<RabbitMqPublisher> _logger;
    
    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _hostname = configuration["RabbitMQ:HostName"] ?? "localhost";
        _defaultQueue = configuration["RabbitMQ:RequestQueue"] ?? "code_execution_requests";
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        await _channel.QueueDeclareAsync(
            queue: _defaultQueue, 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQPublisher started. Host: {Host}, Queue: {Queue}", _hostname, _defaultQueue);
    }
    
    public async Task PublishAsync<T>(T message, string? routingKey = null)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogError("RabbitMQ channel is not open. Cannot publish message.");
            throw new InvalidOperationException("Channel is closed");
        }

        var targetQueue = routingKey ?? _defaultQueue;
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        var properties = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

        await _channel.BasicPublishAsync(
            exchange: string.Empty, 
            routingKey: targetQueue, 
            mandatory: true, 
            basicProperties: properties, 
            body: body);

        _logger.LogInformation("Published message to {Queue}: {Content}", targetQueue, json);
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down RabbitMQPublisher...");
        await DisposeAsync();
    }
    public async ValueTask DisposeAsync()
    {
        if (_channel is { IsOpen: true }) await _channel.CloseAsync();
        if (_connection is { IsOpen: true }) await _connection.CloseAsync();
    }
}