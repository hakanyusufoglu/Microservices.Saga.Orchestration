using MassTransit;
using Payment.Api.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentStartedEventConsumer>();

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMq"]);

        _configure.ReceiveEndpoint(RabbitMqSettings.Payment_StartedEventQueue, e => e.ConfigureConsumer<PaymentStartedEventConsumer>(context));
    });
});

var app = builder.Build();

app.Run();
