using FileCreateWorkerService;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var configsetting=new ConfigurationBuilder().Build();
        services.AddHostedService<Worker>();
        services.AddSingleton(sp => new ConnectionFactory()
        {
            Uri = new Uri(configsetting.GetConnectionString("RabbitMQ")),
            DispatchConsumersAsync = true
        });
        services.AddSingleton<RabbitMQClientService>();
        services.AddDbContext<AdventureWorks2019Context>(opt=>
        {
            opt.UseSqlServer(configsetting.GetConnectionString("ConnectionString"));
        });


    })
    .Build();

host.Run();
