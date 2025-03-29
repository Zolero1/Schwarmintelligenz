using RabbitMQ.Client;

namespace GlobalClasses;

public class RabbitMQPersistentConnection : IDisposable
{
    public IConnection _connection { get; private set; }
    public IModel _channel { get; private set; }
    private string _queueName = "commandQueue";

    public RabbitMQPersistentConnection()
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672, DispatchConsumersAsync = true};
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _queueName,   // Name der Queue
            durable: true,         // Die Queue bleibt nach einem Neustart bestehen
            exclusive: false,      // Kann von mehreren Verbindungen genutzt werden
            autoDelete: false,     // Nicht automatisch löschen, wenn keine Consumer mehr verbunden sind
            arguments: null        // Zusätzliche Argumente (z. B. TTL, max. Nachrichten)
        );
    }
    public void Dispose()
    {
        _connection.Dispose();
        _channel.Dispose();
    }

    public void Main()
    {
        
    }
}