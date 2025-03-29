using System.Text;
using GlobalUsings;
using RabbitMQ.Client;

namespace CentralService;

public class RabbitQueueSender : IDisposable
{
    private  IModel _channel;

    public RabbitQueueSender()
    {
        _channel = RabbitMQPersistentConnection.Instance.GetChannel();
    }
    
    
    /*public async Task Initiate()
    {
        var factory = new ConnectionFactory { HostName = "goalQueue", Port = 5672};
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }*/

    public void SendToDrone(string[] messages)
    {
        foreach (var message in messages)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: RabbitMQPersistentConnection.Instance.ExchangeName,  routingKey: RabbitMQPersistentConnection.Instance.DroneRoutingKey, body: body);

            Console.WriteLine($"Sent: {message}");
        }
    }
    public void SendToDrone(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        string exchangeName = RabbitMQPersistentConnection.Instance.ExchangeName;
        _channel.BasicPublish(exchange: exchangeName, routingKey: RabbitMQPersistentConnection.Instance.DroneRoutingKey, body: body);

        Console.WriteLine($"Sent: {message} to {exchangeName} with routing key {RabbitMQPersistentConnection.Instance.DroneRoutingKey}");
    }

    public void SendToCentral(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        string exchangeName = RabbitMQPersistentConnection.Instance.ExchangeNameZentral;
        _channel.BasicPublish(exchange: exchangeName, routingKey: RabbitMQPersistentConnection.Instance.ZentralRoutingKey, body: body);

        Console.WriteLine($"Sent: {message} to {exchangeName} with routing key {RabbitMQPersistentConnection.Instance.ZentralRoutingKey}");
    }

    public IModel GetChannel() => _channel;

    public void Dispose()
    {
        _channel?.Close();
    }
}


