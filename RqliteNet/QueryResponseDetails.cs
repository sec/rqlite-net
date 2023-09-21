using System.Text.Json;

namespace RqliteNet;

public record QueryResponseDetails(List<string> Types, List<string> Columns, List<List<JsonElement>> Values, string? Error);