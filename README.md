# Transaction Outbox Pattern using MassTransit over RabbitMQ
This repository demonstrates the implementation of the Transaction Outbox Pattern using MassTransit over RabbitMQ. Specifically, it shows how to trigger an email when a customer is created.

## Prerequisites
- Install Masstransit nuget pkg
- Install Masstransit.RabbitMQ nuget pkg
- Install Postgres nuget pkg

## Setup
A docker-compose file is provided to run Postgress and RabbitMQ services in a docker container. Run this file using **docker-compose up** command

### Transaction Outbox Pattern

The Transaction Outbox Pattern is implemented using MassTransit's native support. When a customer is created, the event is not directly published. Instead, it is stored in an outbox. After the local transaction is successfully completed, the outbox is committed and the event is published.
When the local transaction is completed, the outbox is committed, and the `CustomerCreatedEvent` is published:

```
x.AddEntityFrameworkOutbox<AppDbContext>(o =>
{
    o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
    o.UsePostgres();
});

x.SetKebabCaseEndpointNameFormatter();

var assembly = typeof(Program).Assembly;
   
x.AddConsumer<CustomerCreatedConsumer>();
x.AddActivities(assembly);   

x.UsingRabbitMq((context, configuration) =>
{ 
    configuration.Host(new Uri("amqp://localhost:5672"), h =>
    {
        h.Username("guest");
        h.Password("guest");
    });

    configuration.ConfigureEndpoints(context);
});
```

In this example, the outbox is stored in Outbox table in Postgres db.

## Usage
1. Configure Mass Transit and the underlying RabbitMq in Program.cs as shown below. Note that we are configuring Masstransit to use outbox.

```csharp
builder.Services.AddMassTransit(bus =>
{
    bus.SetKebabCaseEndpointNameFormatter();

    bus.AddEntityFrameworkOutbox<ApplicationDbContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(1);
        options.UsePostgres().UseBusOutbox();
    });

    bus.UsingRabbitMq((context, configuration) =>
    {
        var settings = context.GetRequiredService<MessageBrokerSettings>();

        configuration.Host(new Uri(settings.Host), h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });

        configuration.ConfigureEndpoints(context);
    });        
});
```

2. Use the API endpoint `/api/customer/CreateCustomer` to create a new customer. This will trigger the **CustomerCreatedEvent**.

```csharp
[HttpPost("CreateCustomer")]
public async Task<IActionResult> CreateCustomer(Customer customer)
{
    if (ModelState.IsValid)
    {
        customer.CustomerId = Guid.NewGuid();

        await appDbContext.Customers.AddAsync(customer);

        var message = new CustomerCreated(customer.CustomerId, customer.CustomerName, customer.Email);

        await publishEndpoint.Publish(message);

        await appDbContext.SaveChangesAsync();

        return Ok("Customer created successfully");
    }
    return BadRequest(ModelState);
}
```

3. When a customer is created, a `CustomerCreatedEvent` is published. This event is handled by the `CustomerCreatedConsumer`.

```csharp
public class CustomerCreatedEventConsumer : IConsumer<CustomerCreatedEvent>
{
    public Task Consume(ConsumeContext<CustomerCreated> context)
    {
        logger.LogInformation("The following customer is created");

        logger.LogInformation(context.Message.CustomerId.ToString()); 
        logger.LogInformation(context.Message.CustomerName.ToString()); 
        logger.LogInformation(context.Message.CustomerEmail.ToString());
        // Send Email
        return Task.CompletedTask;   
    }
}
```

## Conclusion

The Transaction Outbox Pattern is a powerful way to ensure consistency between local transactions and published events. This repository demonstrates how to implement this pattern using MassTransit's native support over RabbitMQ.
