namespace GameBook.Contracts.Vapi;

/// <summary>
/// Internal result from a Vapi tool handler.
/// The webhook converts this to the Vapi response shape before returning.
/// </summary>
public sealed record VapiToolResult(
    bool Success,
    string Message,
    string? Error = null);
