using IndustryTrade.Worker;
using MassTransit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(cfg => cfg
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbit = builder.Configuration.GetConnectionString("RabbitMq")
                     ?? "amqp://guest:guest@localhost:5672";
        cfg.Host(new Uri(rabbit));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<OutboxDispatcher>();

var host = builder.Build();
host.Run();
