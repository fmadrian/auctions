using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(
    (options) =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
);
// Inject Automapper as a Service
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Add MassTransit configuration
builder.Services.AddMassTransit((x) =>
{
    // Set an outbox (and pass the service database context).
    x.AddEntityFrameworkOutbox<ApplicationDbContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(10); // Once the service bus is available, each 10 seconds attempts to deliver the message.
        options.UsePostgres(); // Use a Postgres database.
        options.UseBusOutbox();
    });
    // Configure consumers.
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    // Add formatter for endpoints. (auction-event)
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    x.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

/*
     Configure the HTTP request pipeline.
         Allows us to add middleware
*/

app.UseAuthorization();

app.MapControllers();
// Seed data using DBInitializer
try
{
    DBInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}
app.Run();
