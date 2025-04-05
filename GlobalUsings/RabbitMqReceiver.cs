using System.Text;
using GlobalUsings;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CentralService;

public class RabbitMqReceiver : IDisposable
{
    private IModel _channel;

    public RabbitMqReceiver()
    {
        _channel = RabbitMQPersistentConnection.Instance.GetChannel();
        _channel.QueueDeclare(
            queue: RabbitMQPersistentConnection.Instance.QueueNameZentral,
            durable: true,
            exclusive: false,
            autoDelete: false
            , arguments: null
        );
        _channel.QueueBind(
            queue: RabbitMQPersistentConnection.Instance.QueueNameZentral,
            exchange: RabbitMQPersistentConnection.Instance.ExchangeNameZentral,
            routingKey: RabbitMQPersistentConnection.Instance.ZentralRoutingKey
            );
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[Central] Message received: {message}");
        };
        _channel.BasicConsume(
            queue: RabbitMQPersistentConnection.Instance.QueueNameZentral,
            autoAck: true,
            consumer: consumer
        );
    }
    
    
    
    public void Dispose()
    {
        _channel.Dispose();
    }
}