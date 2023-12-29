using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.Data.Repositories;
using AuctionService.Data.Repositories.Interfaces;
using AuctionService.Entities;
using AuctionService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
// Add gRPC service
builder.Services.AddGrpc();

// Add/Link repositories
// Has to be on the same scope as the database context.
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
var app = builder.Build();

/*
     Configure the HTTP request pipeline.
         Allows us to add middleware
*/
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Add mapping of gRPC service
app.MapGrpcService<GrpcAuctionService>();
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
