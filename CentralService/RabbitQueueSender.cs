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
        SendMessages("Test");
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

    public void SendMessages(string[] messages)
    {
        foreach (var message in messages)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",  routingKey: RabbitMQPersistentConnection.Instance.GetQueueName(), body: body);

            Console.WriteLine($"Sent: {message}");
        }
    }
    public void SendMessages(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: string.Empty, routingKey: RabbitMQPersistentConnection.Instance.GetQueueName(), body: body);

        Console.WriteLine($"Sent: {message}");
    }

    public IModel GetChannel() => _channel;

    public void Dispose()
    {
        _channel?.Close();
    }
}


