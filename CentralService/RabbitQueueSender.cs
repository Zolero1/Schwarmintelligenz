using System.Text;
using RabbitMQ.Client;

namespace CentralService;

public class RabbitQueueSender
{
    private  IConnection _connection;
    private  IChannel _channel;
    private  string _queueName = "commandQueue";
    
    public async Task Initiate()
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672/")
        };
        
        
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task SendMessages(string[] messages)
    {
        await _channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);
        foreach (var message in messages)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(exchange: "logs", routingKey: "hello", body: body);

            Console.WriteLine($"Sent: {message}");
        }
    }
    public async Task SendMessages(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);

        Console.WriteLine($"Sent: {message}");
    }
}