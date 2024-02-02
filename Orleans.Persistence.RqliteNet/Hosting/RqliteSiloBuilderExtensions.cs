using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.RqliteNet.Providers;
using Orleans.Persistence.RqliteNet.Storage;
using Orleans.Providers;
using Orleans.Runtime.Hosting;
using Orleans.Storage;

namespace Orleans.Persistence.RqliteNet.Hosting;

public static class RqliteSiloBuilderExtensions
{
    public static ISiloBuilder AddRqliteGrainStorageAsDefault(this ISiloBuilder builder, Action<RqliteGrainStorageOptions> options)
    {
        return builder.AddRqliteGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, options);
    }

    public static ISiloBuilder AddRqliteGrainStorage(this ISiloBuilder builder, string providerName, Action<RqliteGrainStorageOptions> options)
    {
        return builder.ConfigureServices(services => services.AddRqliteGrainStorage(providerName, options));
    }

    public static IServiceCollection AddRqliteGrainStorage(this IServiceCollection services, string providerName, Action<RqliteGrainStorageOptions> options)
    {
        services.AddOptions<RqliteGrainStorageOptions>(providerName).Configure(options);

        services.AddTransient<IPostConfigureOptions<RqliteGrainStorageOptions>, DefaultStorageProviderSerializerOptionsConfigurator<RqliteGrainStorageOptions>>();

        return services.AddGrainStorage(providerName, RqliteGrainStorageFactory.Create);
    }
}