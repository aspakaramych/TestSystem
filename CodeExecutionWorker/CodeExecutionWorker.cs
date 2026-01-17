using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CodeExecutionWorker;
public class CodeExecutionWorker : BackgroundService
{
    private readonly RabbitMqPublisher _publisher; 
    private readonly DockerService _dockerService;
    private readonly ILogger<CodeExecutionWorker> _logger;
    private readonly string _hostname;
    private readonly string _requestQueue;
    
    private IConnection? _connection;
    private IChannel? _channel;

    public CodeExecutionWorker(
        IConfiguration configuration, 
        RabbitMqPublisher publisher, 
        DockerService dockerService, 
        ILogger<CodeExecutionWorker> logger)
    {
        _publisher = publisher;
        _dockerService = dockerService;
        _logger = logger;
        
        _hostname = configuration["RabbitMQ:HostName"] ?? "rabbitmq";
        _requestQueue = configuration["RabbitMQ:RequestQueue"] ?? "code_executor_request";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: _requestQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    _logger.LogInformation("Received task from RabbitMQ");
                    
                    await ProcessTaskAsync(json);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing task from RabbitMQ");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, stoppingToken);
                }
            };
            
            await _channel.BasicConsumeAsync(queue: _requestQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            _logger.LogInformation("Worker started. Listening to queue: {Queue}", _requestQueue);
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in RabbitMQ Worker");
        }
    }

    private async Task ProcessTaskAsync(string json)
    {
        var request = JsonSerializer.Deserialize<CodeExecutionRequest>(json);
        if (request == null) return;

        _logger.LogInformation("Executing code for PackageId: {Id}", request.PackageId);
        
        var result = await _dockerService.ExecuteCodeAsync(request);

        await _publisher.ProduceAsync(result);   
    }

    public override async void Dispose()
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        base.Dispose();
    }
}