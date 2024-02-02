using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Persistence.RqliteNet.Providers;

namespace Orleans.Persistence.RqliteNet.Storage;

public static class RqliteGrainStorageFactory
{
    public static RqliteGrainStorage Create(IServiceProvider service, string name)
    {
        var options = service.GetRequiredService<IOptionsMonitor<RqliteGrainStorageOptions>>();

        return ActivatorUtilities.CreateInstance<RqliteGrainStorage>(service, name, options.Get(name), service.GetProviderClusterOptions(name));
    }
}