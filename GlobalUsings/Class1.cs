using RabbitMQ.Client;

namespace GlobalUsings;

public sealed class RabbitMQPersistentConnection : IDisposable
{
    private static readonly Lazy<RabbitMQPersistentConnection> _instance =
        new Lazy<RabbitMQPersistentConnection>(() => new RabbitMQPersistentConnection());

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName = "commandQueue";
    private bool _disposed = false;

    public static RabbitMQPersistentConnection Instance => _instance.Value;

    private RabbitMQPersistentConnection()
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672, DispatchConsumersAsync = true };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public IModel GetChannel() => _channel;
    public string GetQueueName() => _queueName;

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