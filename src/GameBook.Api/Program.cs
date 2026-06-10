using DotNetEnv;
using GameBook.Api.Endpoints;
using GameBook.Api.Hubs;
using GameBook.Api.Jobs;
using GameBook.Application;
using GameBook.Domain.Entities;
using GameBook.Infrastructure;
using GameBook.Infrastructure.Persistence;
using Meshcaster.IdentityProvider.Extensions;
using Serilog;

Env.Load();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console());

    // Application & Infrastructure
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Auth & Identity
    var supabaseUrl = builder.Configuration["SUPABASE_URL"]
        ?? Environment.GetEnvironmentVariable("SUPABASE_URL");

    builder.Services.AddMeshcasterIdentity<User, GameBookDbContext>(options =>
    {
        options.JwtSecret = builder.Configuration["SUPABASE_JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")
            ?? "super-secret-jwt-token-for-development-only";
        options.JwksUrl = supabaseUrl is not null
            ? $"{supabaseUrl.TrimEnd('/')}/auth/v1/.well-known/jwks.json"
            : null;
        options.SignalRPathSegments = ["/hubs"];
    });

    // SignalR
    var redisUrl = builder.Configuration["REDIS_URL"]
        ?? Environment.GetEnvironmentVariable("REDIS_URL");

    var signalR = builder.Services.AddSignalR();
    if (!string.IsNullOrEmpty(redisUrl))
    {
        signalR.AddStackExchangeRedis(redisUrl);
    }

    // OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "GameBook API";
        config.Version = "v1";

        config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
        {
            Type = NSwag.OpenApiSecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
        });

        config.OperationProcessors.Add(
            new NSwag.Generation.Processors.Security.OperationSecurityScopeProcessor("Bearer"));
    });

    // Rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
            context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                }));
        options.RejectionStatusCode = 429;
    });

    // Background jobs
    builder.Services.AddHostedService<NoShowSweeper>();
    builder.Services.AddHostedService<BookingReminderJob>();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });

    var app = builder.Build();

    // Middleware pipeline
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseRateLimiter();
    app.UseMeshcasterIdentity<User, GameBookDbContext>();

    // OpenAPI / Swagger
    app.UseStaticFiles();
    app.UseOpenApi();
    app.UseSwaggerUi();

    // Endpoints
    app.MapVenueEndpoints();
    app.MapBookingEndpoints();
    app.MapPaymentEndpoints();
    app.MapUserEndpoints();
    app.MapReviewEndpoints();
    app.MapWebhookEndpoints();
    app.MapVapiEndpoints();

    // SignalR hubs
    app.MapHub<VenueHub>("/hubs/venue");
    app.MapHub<UserHub>("/hubs/user");

    // Health check
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

    // Database setup
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GameBookDbContext>();
    await db.Database.EnsureCreatedAsync();

    // Seed data only in development
    var csvPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "playstation_lounges_georgia.csv"));
    if (File.Exists(csvPath))
        await SeedData.SeedAsync(db, csvPath);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the Program class accessible for integration tests
public partial class Program;
