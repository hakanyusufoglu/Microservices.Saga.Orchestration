using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.StateMaps;

namespace SagaStateMachine.StateDbContexts
{
    //SagaDbContext şemsına uygun olarak OrderStateMap'i ekliyoruz.
    public class OrderStateDbContext : SagaDbContext
    {
        public OrderStateDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations {

            get
            {
                //Validasyonu geçerli kılıyoruz.
                yield return new OrderStateMap();
            }
        }
    }
}
