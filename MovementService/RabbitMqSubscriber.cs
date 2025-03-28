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

public class RabbitMqSubscriber : BackgroundService 
{
    private IModel _channel;

    public RabbitMqSubscriber()
    {
        _channel = RabbitMQPersistentConnection.Instance.GetChannel();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");
        }
        
        // This is where the background task runs, continually consuming messages
        stoppingToken.Register(()=> _channel.Close());

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>  
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[Consumer] Received: {message}");
            
            List<string> coordinats = message.Split(',').ToList();
            if (coordinats.Count == 3)
            {
                // TODO Dronenwert Anpassen
            }
            else
            {
                Console.WriteLine($"Message {message} could not be processed. Not in the correct format: int,int,int");
            }
            
            return Task.CompletedTask;
        };
        
        try
        {
            _channel.BasicConsume(queue: RabbitMQPersistentConnection.Instance.GetQueueName(),
                autoAck: true,
                consumer: consumer);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        await Task.CompletedTask;
    }  
}


