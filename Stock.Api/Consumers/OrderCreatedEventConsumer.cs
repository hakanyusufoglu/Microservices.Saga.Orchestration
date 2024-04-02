using MassTransit;
using MongoDB.Driver;
using Shared.OrderEvents;
using Shared.Settings;
using Shared.StockEvents;
using Stock.Api.Services;

namespace Stock.Api.Consumers
{
    public class OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResults = new();
            var stockCollection = mongoDbService.GetCollection<Stock.Api.Models.Stock>();
            //Stock varsa true yoksa false döner ve stockResult listesine eklenir.
            foreach (var orderItem in context.Message.OrderItems)
                stockResults.Add(await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= (long) orderItem.Count)).AnyAsync());

            var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));

            if (stockResults.TrueForAll(s => s.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await (await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;

                    await stockCollection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId, stock);
                }
                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };

                await sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stock not reserved"
                };

                await sendEndpoint.Send(stockNotReservedEvent);
            }
        }
    }
}
