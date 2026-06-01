using GameBook.Application.Common.Interfaces;
using GameBook.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Api.Endpoints;

public static class WebhookEndpoints
{
    public static RouteGroupBuilder MapWebhookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/webhooks").WithTags("Webhooks");

        group.MapPost("/stripe", async (HttpContext context, IGameBookDbContext db, IConfiguration config) =>
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            // In production, verify Stripe signature here
            // var stripeEvent = EventUtility.ConstructEvent(json, context.Request.Headers["Stripe-Signature"], config["Stripe:WebhookSecret"]);

            // Simplified: mark payment as succeeded
            // Real implementation would parse the Stripe event
            return Results.Ok(new { received = true });
        }).WithName("StripeWebhook");

        return group;
    }
}
