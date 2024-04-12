using Customers.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Customers.Worker;

internal class CustomerCreatedConsumer : IConsumer<CustomerCreated>
{
    private readonly ILogger<CustomerCreatedConsumer> logger;

    public CustomerCreatedConsumer(ILogger<CustomerCreatedConsumer> logger)
    {
        this.logger = logger;
    }
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
