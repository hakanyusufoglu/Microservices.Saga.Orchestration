using MassTransit;
using Shared.PaymentEvents;
using Shared.Settings;

namespace Payment.Api.Consumers
{
    public class PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider) : IConsumer<PaymentStartedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var sendEndPoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));
            // işlem başarılıymış gibi davranıyoruz ve PaymentCompletedEvent oluşturuyoruz. 
            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId)
                {
                };

                await sendEndpointProvider.Send(paymentCompletedEvent);
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Payment failed",
                    OrderItems = context.Message.OrderItems
                };

                await sendEndpointProvider.Send(paymentFailedEvent);

            }
        }
    }
}
