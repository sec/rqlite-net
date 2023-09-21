namespace Orleans.Persistence.RqliteNet.Storage;

internal class StateDto
{
    public string GrainId { get; set; } = string.Empty;

    public string ETag { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
}