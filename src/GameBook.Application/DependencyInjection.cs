using FluentValidation;
using GameBook.Application.Services;
using GameBook.Application.Vapi;
using Microsoft.Extensions.DependencyInjection;

namespace GameBook.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<PricingService>();

        // Vapi tool handlers — each IVapiToolHandler is auto-discovered
        var assembly = typeof(DependencyInjection).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IVapiToolHandler).IsAssignableFrom(t));

        foreach (var type in handlerTypes)
            services.AddScoped(typeof(IVapiToolHandler), type);

        services.AddScoped<IVapiToolDispatcher, VapiToolDispatcher>();

        return services;
    }
}
