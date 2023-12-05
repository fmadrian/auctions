using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
// Inject Automapper as a Service
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Add MassTransit configuration
builder.Services.AddMassTransit((x) =>
{
    // Where to find consumers.
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    // Formatting for endpoints (don't include namespaces).
    // Kebab case: word1-word2
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false)); // Example: AuctionCreated = search-auction-created.
    x.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            // Guest is a default value.
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ReceiveEndpoint("search-auction-created", (endpoint) =>
        { // For this endpoint.
            endpoint.UseMessageRetry(retry =>
            {
                retry.Interval(5, 5); // Retry 5 times, wait 5 seconds between each attempt.
            });
            endpoint.ConfigureConsumer<AuctionCreatedConsumer>(context); // Only applies to AuctionCreatedConsumer.
        });
        config.ConfigureEndpoints(context);

    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(
    async () =>
    {
        try
        {
            // Initialize MongoDB and seed data.
            await DBInitializer.InitDb(app);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
);

app.Run();

// Create policy for HTTP Client that connects with AuctionService.
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError() // Handle temporary errors.
                                    // .OrResult(msg=>msg.StatusCode == HttpStatusCode.NotFound) // Handle 404 errors (not necessary)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); // What to do? (repeat each 3 seconds until it succeeds)