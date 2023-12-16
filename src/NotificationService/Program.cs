using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 3. Add MassTransit configuration
builder.Services.AddMassTransit((x) =>
{
    // Register consumers.
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    // Add formatter for endpoints.
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifications", false));
    x.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            // Guest is a default value.
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ConfigureEndpoints(context);
    });
});

// 1. Add SignalR service.
builder.Services.AddSignalR();

var app = builder.Build();

// 2. Tell application where the hub is.
app.MapHub<NotificationHub>("/notifications");

app.Run();
