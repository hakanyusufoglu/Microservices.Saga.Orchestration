using MassTransit;
using Order.Api.Contexts;
using Shared.OrderEvents;

namespace Order.Api.Consumers
{
    public class OrderFailedEventConsumer(OrderDbContexts orderDbContext) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Order.Api.Models.Order order = await orderDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatus = Enums.OrderStatus.Fail;
                await orderDbContext.SaveChangesAsync();
            }
        }
    }
}
