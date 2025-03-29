using RabbitMQ.Client;

namespace GlobalUsings;

public sealed class RabbitMQPersistentConnection : IDisposable
{
    private static readonly Lazy<RabbitMQPersistentConnection> _instance =
        new Lazy<RabbitMQPersistentConnection>(() => new RabbitMQPersistentConnection());

    private readonly IConnection _connection;
    private readonly IModel _channel;
    public string QueueNameZentral { get; } = "commandQueue";
    public string ExchangeName { get; } = "drone_topic";
    public string ExchangeNameZentral { get; } = "zentral_direct";
    public string DroneRoutingKey { get; } = "drone.command";
    public string ZentralRoutingKey { get; } = "zentral.reply";
    private bool _disposed = false;

    public static RabbitMQPersistentConnection Instance => _instance.Value;
    
    private RabbitMQPersistentConnection()
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672, DispatchConsumersAsync = true };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true
        );
        _channel.ExchangeDeclare(
            exchange: ExchangeNameZentral
            , type: ExchangeType.Direct
            , durable: true
            );
        
        
    }

    public IModel GetChannel() => _channel;

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}