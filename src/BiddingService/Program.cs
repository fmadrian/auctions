using BiddingService.Consumers;
using BiddingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add MassTransit configuration
builder.Services.AddMassTransit((x) =>
{
    // Add all the consumers we create for this service.
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    // Add formatter for endpoints. (auction-event)
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
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
// Configure JWT service.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Authority is our Identity server.
                    options.Authority = builder.Configuration["IdentityServiceUrl"]; // Identity Service (IdentityServer)
                    options.RequireHttpsMetadata = false; // Identity server is running on HTTP (at the moment).
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.NameClaimType = "username";

                });

// Automapper configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add background services.
builder.Services.AddHostedService<CheckAuctionFinished>();
builder.Services.AddScoped<GrpcAuctionClient>();
var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// Initialize database
await DB.InitAsync("BidDb", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));


app.Run();
