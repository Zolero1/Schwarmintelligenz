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
d1.Initialize();
Drone d2 = new Drone("D2");
d2.Initialize();
Drone d3 = new Drone("D3");
d3.Initialize();
Drone d4 = new Drone("D4");
d4.Initialize();
await host.RunAsync();