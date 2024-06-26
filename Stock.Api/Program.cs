using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.Api.Consumers;
using Stock.Api.Models;
using Stock.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollbackMessageConsumer>();

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMq"]);

        _configure.ReceiveEndpoint(RabbitMqSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMqSettings.Stock_RollbackMessageQueue, e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

// Sorgu išerisnde veri varsa dummy data eklemiyoruz.
if (!await (await mongoDbService.GetCollection<Stock.Api.Models.Stock>().FindAsync(x => true)).AnyAsync())
{
    await mongoDbService.GetCollection<Stock.Api.Models.Stock>().InsertOneAsync(new Stock.Api.Models.Stock()
    {
        ProductId = 1,
        Count = 200,
    });

    await mongoDbService.GetCollection<Stock.Api.Models.Stock>().InsertOneAsync(new Stock.Api.Models.Stock()
    {
        ProductId = 2,
        Count = 300,
    });

    await mongoDbService.GetCollection<Stock.Api.Models.Stock>().InsertOneAsync(new Stock.Api.Models.Stock()
    {
        ProductId = 3,
        Count = 50,
    });

    await mongoDbService.GetCollection<Stock.Api.Models.Stock>().InsertOneAsync(new Stock.Api.Models.Stock()
    {
        ProductId = 4,
        Count = 10,
    });

    await mongoDbService.GetCollection<Stock.Api.Models.Stock>().InsertOneAsync(new Stock.Api.Models.Stock()
    {
        ProductId = 5,
        Count = 60,
    });
}

app.Run();
