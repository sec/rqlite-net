using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Persistence.RqliteNet.Providers;
using Orleans.Persistence.RqliteNet.Storage;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Persistence.RqliteNet.Hosting;

public static class RqliteSiloBuilderExtensions
{
    public static ISiloBuilder AddRqliteGrainStorage(this ISiloBuilder builder, string providerName, Action<RqliteGrainStorageOptions> options)
    {
        return builder.ConfigureServices(services => services.AddRqliteGrainStorage(providerName, options));
    }

    public static IServiceCollection AddRqliteGrainStorage(this IServiceCollection services, string providerName, Action<RqliteGrainStorageOptions> options)
    {
        services.AddOptions<RqliteGrainStorageOptions>(providerName).Configure(options);

        services.AddTransient<IPostConfigureOptions<RqliteGrainStorageOptions>, DefaultStorageProviderSerializerOptionsConfigurator<RqliteGrainStorageOptions>>();

        if (string.Equals(providerName, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.Ordinal))
        {
            services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
        }

        return services
            .AddSingletonNamedService(providerName, RqliteGrainStorageFactory.Create)
            .AddSingletonNamedService(providerName, (p, n) => (ILifecycleParticipant<ISiloLifecycle>) p.GetRequiredServiceByName<IGrainStorage>(n));
    }
}
