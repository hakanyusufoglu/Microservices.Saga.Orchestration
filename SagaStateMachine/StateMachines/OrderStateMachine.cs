using MassTransit;
using SagaStateMachine.StateInstances;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
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

        //State tanımlıyoruz böylelikle ilgili eventin state'i ne olacak onu belirliyoruz.

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }

        public OrderStateMachine()
        {
            // Bu kısım, state'lerin tanımlandığı kısım. Db tarafında hangi state'lerin olacağını belirler.
            InstanceState(instance => instance.CurrentState);

            // Eğer ki gelen event OrderStartedEvent ise tetikleyici bir eventtir. Database de bu orderIdye ait bir state yoksa yeni bir correlationId oluşturur.
            Event(() => OrderStartedEvent, orderStateInstance => orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId).SelectId(e => Guid.NewGuid()));

            // hangi instance karşı bu event gerçekleştiriliyor. sorusuna cevap verir.
            Event(() => StockReservedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
            Event(() => StockNotReservedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
            Event(() => PaymentCompletedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
            Event(() => PaymentFailedEvent, orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));


            // State'lerin tanımlandığı kısım. ve statein başladığı durum
            Initially(When(OrderStartedEvent).Then(context =>
            {
                context.Instance.OrderId = context.Data.OrderId; //context.Instance = veritabanındaki state'i temsil eder. context.Data = gelen eventi temsil eder.
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.TotalPrice = context.Data.TotalPrice;
                context.Instance.CreatedDate = DateTime.UtcNow;
            })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems,
                }));

            //Gelen siparişin eventi OrderCreated ise tetiklenir.
            During(OrderCreated, When(StockReservedEvent).TransitionTo(StockReserved).Send(new Uri($"queue:{RabbitMqSettings.Payment_StartedEventQueue}"),
                context => new PaymentStartedEvent(context.Instance.CorrelationId)
                {
                    TotalPrice = context.Instance.TotalPrice,
                    OrderItems = context.Data.OrderItems
                }),
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderFailedEventQueue}"), context => new OrderFailedEvent
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                }));

            During(StockReserved,
                When(PaymentCompletedEvent).TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderCompletedEventQueue}"),
                context => new OrderCompletedEvent
                {
                    OrderId = context.Instance.OrderId,
                })
                .Finalize(), When(PaymentFailedEvent).TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_OrderFailedEventQueue}"),
                context => new OrderFailedEvent
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                })
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_RollbackMessageQueue}"),
                context => new StockRollbackMessage
                {
                    OrderItems = context.Data.OrderItems
                }));

            // State'lerin sonlandığı kısım
            SetCompletedWhenFinalized();
        }
    }
}
