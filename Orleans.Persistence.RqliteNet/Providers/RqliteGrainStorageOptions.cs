using Orleans.Storage;

namespace Orleans.Persistence.RqliteNet.Providers;

public class RqliteGrainStorageOptions : IStorageProviderSerializerOptions
{
    public required string Uri { get; set; }

    public required IGrainStorageSerializer GrainStorageSerializer { get; set; }
}