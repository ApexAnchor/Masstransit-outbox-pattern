using Customers.Contracts;
using Customers.WebApi.Data;
using Customers.WebApi.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Customers.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ApplicationDbContext appDbContext;
    private readonly IPublishEndpoint publishEndpoint;

    public CustomerController(ApplicationDbContext appDbContext, IPublishEndpoint publishEndpoint)
    {
        this.appDbContext = appDbContext;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpPost("CreateCustomer")]
    public async Task<IActionResult> CreateCustomer(Customer customer)
    {
        if (ModelState.IsValid)
        {
            customer.CustomerId = Guid.NewGuid();
           
            appDbContext.Customers.Add(customer);

            var message = new CustomerCreated(customer.CustomerId, customer.CustomerName, customer.Email);
            
            await publishEndpoint.Publish(message);
            
            await appDbContext.SaveChangesAsync();
                       
            return Ok("Customer created succesfully");
        }
        return BadRequest(ModelState);
    }

}
