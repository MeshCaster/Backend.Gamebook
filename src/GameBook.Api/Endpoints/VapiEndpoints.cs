using System.Security.Cryptography;
using System.Text;
using GameBook.Application.Vapi;
using GameBook.Contracts.Vapi;

namespace GameBook.Api.Endpoints;

public static class VapiEndpoints
{
    public static RouteGroupBuilder MapVapiEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/v1/vapi").WithTags("Vapi");

        group.MapPost("/webhook", async (
            HttpContext context,
            VapiServerMessage body,
            IVapiToolDispatcher dispatcher,
            IConfiguration config,
            ILogger<Program> logger) =>
        {
            // ── Secret validation (constant-time) ──
            var expectedSecret = config["VAPI_SERVER_SECRET"]
                ?? Environment.GetEnvironmentVariable("VAPI_SERVER_SECRET")
                ?? string.Empty;

            var providedSecret = context.Request.Headers["X-Vapi-Secret"].FirstOrDefault() ?? string.Empty;

            if (expectedSecret.Length == 0 ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSecret),
                    Encoding.UTF8.GetBytes(providedSecret)))
            {
                logger.LogWarning("Vapi webhook: secret mismatch");
                return Results.Unauthorized();
            }

            // ── 18-second timeout for the entire request ──
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(18));
            var ct = cts.Token;

            var messageType = body.Message?.Type ?? string.Empty;

            switch (messageType)
            {
                case "tool-calls":
                    return await HandleToolCalls(body, dispatcher, logger, ct);

                case "status-update":
                    logger.LogInformation("Vapi status-update: call {CallId}",
                        body.Message?.Call?.Id);
                    return Results.Ok();

                case "end-of-call-report":
                    logger.LogInformation("Vapi end-of-call-report: call {CallId}",
                        body.Message?.Call?.Id);
                    return Results.Ok();

                case "hang":
                    logger.LogInformation("Vapi hang: call {CallId}",
                        body.Message?.Call?.Id);
                    return Results.Ok();

                // NOTE: assistant-request is NOT handled — tool schemas are configured
                // in the Vapi dashboard, not served from code.

                default:
                    logger.LogDebug("Vapi webhook: ignoring message type {MessageType}", messageType);
                    return Results.Ok();
            }
        }).WithName("VapiWebhook");

        return group;
    }

    private static async Task<IResult> HandleToolCalls(
        VapiServerMessage body,
        IVapiToolDispatcher dispatcher,
        ILogger logger,
        CancellationToken ct)
    {
        var toolCalls = body.Message?.ToolCallList;
        if (toolCalls is null || toolCalls.Count == 0)
        {
            logger.LogWarning("Vapi tool-calls message with empty toolCallList");
            return Results.Ok();
        }

        var callerPhone = body.Message?.Call?.Customer?.Number;

        var results = new List<VapiResponseResult>(toolCalls.Count);
        foreach (var toolCall in toolCalls)
        {
            var toolResult = await dispatcher.DispatchAsync(toolCall.Name, toolCall.Arguments, callerPhone, ct);

            // Vapi requires result and error to be strings
            var resultString = toolResult.Success
                ? toolResult.Message
                : toolResult.Error ?? toolResult.Message;

            results.Add(new VapiResponseResult(toolCall.Id, resultString));
        }

        return Results.Ok(new VapiResponse(results));
    }
}
