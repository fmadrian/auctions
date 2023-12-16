using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Configure JWT service.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                        // Authority is our Identity server.
                        options.Authority = builder.Configuration["IdentityServiceUrl"]; // Identity Service (IdentityServer)
                        options.RequireHttpsMetadata = false; // Identity server is running on HTTP (at the moment).
                        options.TokenValidationParameters.ValidateAudience = false;
                        options.TokenValidationParameters.NameClaimType = "username";

                });

// 3. Add CORS configuration.
builder.Services.AddCors(options =>
{
        // 4.1. Add a custom policy.
        options.AddPolicy("customPolicy", policy =>
        {
                // Allow this into the CORS headers we send back
                policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins(builder.Configuration["ClientApp"]);
        });
});

var app = builder.Build();


// 4. Add the middelware.
app.UseCors();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
