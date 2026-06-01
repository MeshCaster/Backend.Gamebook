using GameBook.Application.Common.Interfaces;
using GameBook.Infrastructure.Persistence;
using GameBook.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameBook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=gamebook;Username=gamebook;Password=gamebook_dev";

        services.AddDbContext<GameBookDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseNetTopologySuite();
                npgsql.MigrationsAssembly(typeof(GameBookDbContext).Assembly.FullName);
            }));

        services.AddScoped<IGameBookDbContext>(sp => sp.GetRequiredService<GameBookDbContext>());

        var stripeKey = configuration.GetValue<string>("STRIPE_SECRET_KEY") ?? "";
        services.AddSingleton<IPaymentService>(new StripePaymentService(stripeKey));

        services.AddHttpClient<IPushNotificationService, ExpoPushService>();

        return services;
    }
}
