using Customers.WebApi.Data;
using Customers.WebApi.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Database"), opt =>
        {
            opt.EnableRetryOnFailure(5);
        });
    });

    builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));

    builder.Services.AddSingleton(sp=>sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

    builder.Services.AddMassTransit(bus =>
    {
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
        });

    });

}



var app = builder.Build();


{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
