using MassTransit;
using SagaStateMachine.StateInstances;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.StockEvents;

namespace SagaStateMachine.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        //Order.Api 'de oluşturduğumuz OrderStartedEvent'i burada tanımlıyoruz.
        //Bu eventler statemachini tarafından tüketilecek eventlerdir.
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        public OrderStateMachine()
        {
            // Bu kısım, state'lerin tanımlandığı kısım. Db tarafında hangi state'lerin olacağını belirler.
            InstanceState(instance => instance.CurrentState);

            // Eğer ki gelen event OrderStartedEvent ise tetikleyici bir eventtir. Database de bu orderIdye ait bir state yoksa yeni bir correlationId oluşturur.
            Event(() => OrderStartedEvent, orderStateInstance => orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId).SelectId(e => Guid.NewGuid()));

            // hangi instance karşı bu event gerçekleştiriliyor. sorusuna cevap verir.
            Event(()=>StockReservedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event=>@event.Message.CorrelationId));
            Event(() => StockNotReservedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
            Event(() => PaymentCompletedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
            Event(() => PaymentFailedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
        }
    }
}
