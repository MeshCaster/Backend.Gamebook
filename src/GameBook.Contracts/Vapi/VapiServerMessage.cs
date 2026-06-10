using System.Text.Json;

namespace GameBook.Contracts.Vapi;

/// <summary>
/// Top-level body POSTed by Vapi to the server URL webhook.
/// Models only the fields we actually read.
/// Docs: https://docs.vapi.ai/tools/custom-tools
/// </summary>
public sealed record VapiServerMessage
{
    public VapiMessagePayload Message { get; init; } = null!;
}

public sealed record VapiMessagePayload
{
    public string Type { get; init; } = string.Empty;

    /// <summary>Tool call list — populated when Type == "tool-calls".</summary>
    public List<VapiToolCallItem>? ToolCallList { get; init; }

    /// <summary>Call metadata including caller phone number.</summary>
    public VapiCallInfo? Call { get; init; }
}

public sealed record VapiToolCallItem
{
    /// <summary>Unique ID that must be echoed back in the response.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Tool name as defined in the Vapi dashboard.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Tool arguments as raw JSON (deserialized per-tool).</summary>
    public JsonElement? Arguments { get; init; }
}

public sealed record VapiCallInfo
{
    public string Id { get; init; } = string.Empty;
    public VapiCustomer? Customer { get; init; }
}

public sealed record VapiCustomer
{
    public string? Number { get; init; }
}
