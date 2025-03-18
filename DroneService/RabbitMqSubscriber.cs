using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MovementService;

public class RabbitMqSubscriber : BackgroundService 
{
    private  IConnection _connection;
    private  IChannel _channel;
    private  string _queueName = "commandQueue";
    
    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672};
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: "logs",
            type: ExchangeType.Fanout);

        QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
        string queueName = queueDeclareResult.QueueName;
        await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // This is where the background task runs, continually consuming messages
        stoppingToken.Register(()=> _channel.CloseAsync(cancellationToken: stoppingToken));

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += (model, ea) =>  
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[Consumer] Received: {message}");
            return Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queue: _queueName,
            autoAck: true,
            consumer: consumer, cancellationToken: stoppingToken);
        await Task.CompletedTask;
    }   
}