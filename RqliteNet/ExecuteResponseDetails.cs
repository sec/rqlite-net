using System.Text.Json.Serialization;

namespace RqliteNet;

public record ExecuteResponseDetails(
    [property: JsonPropertyName("last_insert_id")] int LastInsertId,
    [property: JsonPropertyName("rows_affected")] int RowsAffected,
     string? Error);