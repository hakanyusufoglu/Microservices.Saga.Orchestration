using MassTransit;
using Order.Api.Contexts;
using Shared.OrderEvents;

namespace Order.Api.Consumers
{
    public class OrderCompletedEventConsumer(OrderDbContexts orderDbContext) : IConsumer<OrderCompletedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            Order.Api.Models.Order order = await orderDbContext.Orders.FindAsync(context.Message.OrderId);
            if(order != null)
            {
                order.OrderStatus = Enums.OrderStatus.Completed;
                await orderDbContext.SaveChangesAsync();
            }
        }
    }
}
