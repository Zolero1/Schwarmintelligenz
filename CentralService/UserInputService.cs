namespace CentralService;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class UserInputService : BackgroundService
{
    private readonly ILogger<UserInputService> _logger;
    private readonly RabbitQueueSender _rabbitQueueSender;

    public UserInputService(ILogger<UserInputService> logger, RabbitQueueSender rabbitMqService)
    {
        _logger = logger;
        _rabbitQueueSender = rabbitMqService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Enter message to send (or type 'exit' to quit): ");
            string? input = Console.ReadLine();

            if (input?.ToLower() == "exit")
            {
                _logger.LogInformation("Exiting...");
                break;
            }

            if (!string.IsNullOrWhiteSpace(input))
            {
                _rabbitQueueSender.SendMessages(input);
                _logger.LogInformation($"Sent: {input}");
            }
        }
    }
}
