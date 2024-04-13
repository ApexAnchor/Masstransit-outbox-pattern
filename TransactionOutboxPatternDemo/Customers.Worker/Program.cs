using Customers.Worker;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddDbContext<AppDbContext>(x =>
{
    string connstring = "Server=localhost;Port=5432;Database=customerdb;User ID=sa;Password=password;";
   
    x.UseNpgsql(connstring, opt =>
    {
        opt.EnableRetryOnFailure(5);
    });
});

builder.Services.AddMassTransit(x =>
{
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
});


var app = builder.Build();

app.Run();
