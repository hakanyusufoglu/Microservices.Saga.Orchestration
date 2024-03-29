using MassTransit;
using SagaStateMachine.StateInstances;

namespace SagaStateMachine.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public OrderStateMachine()
        {
            // Bu kısım, state'lerin tanımlandığı kısım. Db tarafında hangi state'lerin olacağını belirler.
            InstanceState(instance=>instance.CurrentState);
        }
    }
}
