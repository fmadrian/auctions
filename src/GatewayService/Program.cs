using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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


var app = builder.Build();

app.MapReverseProxy();

// Add middleware
app.UseAuthentication();
app.UseAuthorization();

app.Run();
