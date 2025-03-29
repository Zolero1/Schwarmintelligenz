// See https://aka.ms/new-console-template for more information

using GlobalUsings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MovementService;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<RabbitMQPersistentConnection>();
    })
    .Build();

Drone d1 = new Drone("D1");
await d1.Initialize();


await host.RunAsync();