using System.Text.Json;
using GameBook.Contracts.Vapi;

namespace GameBook.Application.Vapi;

/// <summary>
/// A single Vapi tool handler. Each tool (search_venues, check_availability, create_booking)
/// implements this interface and is resolved by <see cref="IVapiToolDispatcher"/>.
/// </summary>
public interface IVapiToolHandler
{
    string ToolName { get; }
    Task<VapiToolResult> HandleAsync(JsonElement? arguments, string? callerPhone, CancellationToken ct);
}

/// <summary>
/// Routes a Vapi tool call to the correct <see cref="IVapiToolHandler"/> by name.
/// </summary>
public interface IVapiToolDispatcher
{
    Task<VapiToolResult> DispatchAsync(string toolName, JsonElement? arguments, string? callerPhone, CancellationToken ct);
}
