using System.Text.Json;
using GameBook.Contracts.Vapi;
using Microsoft.Extensions.Logging;

namespace GameBook.Application.Vapi;

public sealed class VapiToolDispatcher : IVapiToolDispatcher
{
    private readonly Dictionary<string, IVapiToolHandler> _handlers;
    private readonly ILogger<VapiToolDispatcher> _logger;

    public VapiToolDispatcher(IEnumerable<IVapiToolHandler> handlers, ILogger<VapiToolDispatcher> logger)
    {
        _handlers = handlers.ToDictionary(h => h.ToolName, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task<VapiToolResult> DispatchAsync(string toolName, JsonElement? arguments, string? callerPhone, CancellationToken ct)
    {
        if (!_handlers.TryGetValue(toolName, out var handler))
        {
            _logger.LogWarning("Unknown Vapi tool requested: {ToolName}", toolName);
            return new VapiToolResult(false, $"Sorry, I don't know how to do that yet.", Error: $"Unknown tool: {toolName}");
        }

        _logger.LogInformation("Dispatching Vapi tool: {ToolName} for caller {CallerPhone}", toolName, callerPhone);
        return await handler.HandleAsync(arguments, callerPhone, ct);
    }
}
