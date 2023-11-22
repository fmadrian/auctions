using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
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