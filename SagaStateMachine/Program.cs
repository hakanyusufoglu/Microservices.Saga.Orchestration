using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.StateDbContexts;
using SagaStateMachine.StateInstances;
using SagaStateMachine.StateMachines;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(options =>
    {
        options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
        {

            _builder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));

        });

    });



    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMq"]);
    });
});

var host = builder.Build();
host.Run();
