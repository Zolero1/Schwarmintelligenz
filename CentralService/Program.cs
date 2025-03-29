// See https://aka.ms/new-console-template for more information

using CentralService;
using GlobalUsings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<RabbitMQPersistentConnection>();
        services.AddSingleton<RabbitQueueSender>();
        services.AddHostedService<UserInputService>();
        //services.AddHostedService<RabbitMqReceiver>();
    })
    .Build();
new RabbitMqReceiver();
/*RabbitQueueSender rabbitQueueSender = host.Services.GetRequiredService<RabbitQueueSender>();
while (true)
{
    rabbitQueueSender.SendMessages("Test");
    Task.Delay(2000).Wait();   
}*/
await host.RunAsync();



