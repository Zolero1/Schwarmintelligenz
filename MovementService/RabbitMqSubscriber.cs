using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalUsings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MovementService;

public class RabbitMqSubscriber : IDisposable
{
    private readonly IModel _channel;
    private readonly string _queueName; // Unique for each subscriber
    
    public event EventHandler<Point> MessageReceived;
    
    public RabbitMqSubscriber(EventHandler<Point> messageReceived)
    {
        MessageReceived = messageReceived;
        _channel = RabbitMQPersistentConnection.Instance.GetChannel();
        
        // Create a unique anonymous queue for this subscriber
        _queueName = _channel.QueueDeclare(
            queue: "", // Let RabbitMQ generate a unique name
            exclusive: true // Auto-delete when disconnected
        ).QueueName;
    }

    public void StartListening()
    {
        _channel.QueueBind(
            queue: _queueName, // Use the unique queue
            exchange: RabbitMQPersistentConnection.Instance.ExchangeName,
            routingKey: RabbitMQPersistentConnection.Instance.DroneRoutingKey // Fanout ignores routing keys
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[Consumer] Received: {message}");
            
            var coordinates = message.Split(',');
            if (coordinates.Length == 3)
            {
                try
                {
                    int X = int.Parse(coordinates[0]);
                    int Y = int.Parse(coordinates[1]);
                    int Z = int.Parse(coordinates[2]);
                    if (X > 0 && Y > 0 && Z > 0 && X < 200 && Y < 200 && Z < 200)
                    {
                        var point = new Point
                        {
                            x = X,
                            y = Y,
                            z = Z
                        };
                        MessageReceived?.Invoke(this, point);
                    }
                    else
                    {
                        Console.WriteLine($"[Consumer] Invalid coordinates: {coordinates[0]}, {coordinates[1]}, {coordinates[2]}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Consumer] Keine Int Werte eingegeben");
                    throw;
                }
            }
            else
            {
                Console.WriteLine($"[Consumer] Invalid message format: {message}");
            }
        };
        
        _channel.BasicConsume(
            queue: _queueName,
            autoAck: true,
            consumer: consumer
        );
    }

    public void Dispose() => _channel?.Dispose();
}




