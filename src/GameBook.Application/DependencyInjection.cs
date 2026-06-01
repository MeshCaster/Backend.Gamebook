using FluentValidation;
using GameBook.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameBook.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<PricingService>();
        return services;
    }
}
