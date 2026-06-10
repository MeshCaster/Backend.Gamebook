namespace GameBook.Contracts.Vapi;

/// <summary>
/// The exact JSON shape Vapi expects back from a tool-calls webhook.
/// Docs: https://docs.vapi.ai/tools/custom-tools
/// </summary>
public sealed record VapiResponse(List<VapiResponseResult> Results);

public sealed record VapiResponseResult(string ToolCallId, string Result);
