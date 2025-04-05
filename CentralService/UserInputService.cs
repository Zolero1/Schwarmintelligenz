using GlobalUsings;
using MovementService;

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
    private readonly DroneFactory _droneFactory;
    private int _serialnumber = 0;

    public UserInputService(ILogger<UserInputService> logger, RabbitQueueSender rabbitMqService)
    {
        _logger = logger;
        _rabbitQueueSender = rabbitMqService;
        _droneFactory = new DroneFactory();
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
                if (input == "start")
                {
                    await _droneFactory.CreateDrone(_serialnumber.ToString());
                    _serialnumber++;
                }
                else
                {
                    _rabbitQueueSender.SendToDrone(input);
                }

                _logger.LogInformation($"Sent: {input}");
            }
        }
    }
}
