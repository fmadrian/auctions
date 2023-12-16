using MassTransit;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add MassTransit configuration
builder.Services.AddMassTransit((x) =>
{
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

// Add SignalR service.
builder.Services.AddSignalR();

var app = builder.Build();

// Tell application where the hub is.
app.MapHub<NotificationHub>("/notifications");

app.Run();
