using MassTransit;

namespace SagaStateMachine.StateInstances
{
    public class OrderStateInstance : SagaStateMachineInstance
    {
        // Bu kısım, state'in durumunu tutan kısım.
        public Guid CorrelationId { get; set; }
        //siparişa dair bilgileri tutabiliriz
        public string CurrentState { get; set; }
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
