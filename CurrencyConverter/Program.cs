
using CurrencyConverter.Middleware;
using CurrencyConverter.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Polly.Extensions.Http;
using Polly;
using Serilog;
using System.Text;
using System.Net;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

//API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
});


//jwt token section
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //Added below code to check if it fails to authenticate
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine(context.Exception);
            return Task.CompletedTask;
        }
    };
    // JWT Token validation
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


builder.Services.AddControllers(options =>
{

});

// Open telemetry implementation
builder.Services.AddOpenTelemetry()
    .WithTracing(tracer =>
    {
        tracer
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// Add Serilog with seq
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:1234")// Replace with log server url
    .CreateLogger();

builder.Host.UseSerilog();
// in memory caching
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICurrencyAPIHelper, CurrencyAPIHelper>();
builder.Services.AddSingleton<ICacheHelper, CacheHelper>();

builder.Services.AddScoped<IJwtService, JwtService>();

// Applly rate limit
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", config =>
    {
        config.PermitLimit = 10; 
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 10;
    });
});
// we have tested the below policy with mock server creation with postman
// Add Retry Policy
static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx + network errors + 408
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) 
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // 2s, 4s, 8s
        );
}

// Added Circuit Breaker
static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
}
builder.Services.AddHttpClient("ExternalApi", client =>
{
        client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Register global exception  middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();


// Apply the named rate limit policy to all controller endpoints
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("default");
app.Run();
