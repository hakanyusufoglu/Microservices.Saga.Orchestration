using MassTransit;
using Shared.Messages;

namespace Shared.OrderEvents
{
    //CorrelatedBy interface'i ile event'ın hangi saga instance ile ilişkilendirileceğini belirtiyoruz.
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
