using Microsoft.Extensions.Options;

namespace RqliteNet.AspNet;

public static class RqliteNetBuilderExtensions
{
    const string RqliteNetClientName = $"{nameof(RqliteNetClient)}HttpClient";

    public static IServiceCollection AddRqliteNet(this IServiceCollection services, Action<RqliteNetOptions> options)
    {
        services.AddHttpClient(RqliteNetClientName);

        services.AddOptions<RqliteNetOptions>().Configure(options);

        services.AddScoped<IRqliteNetClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var opt = sp.GetRequiredService<IOptionsMonitor<RqliteNetOptions>>();

            ArgumentNullException.ThrowIfNull(opt.CurrentValue.Uri, "RqliteNetOptions.Uri");

            var http = factory.CreateClient(RqliteNetClientName);
            http.BaseAddress = new(opt.CurrentValue.Uri);

            return new RqliteNetClient(string.Empty, http);
        });

        return services;
    }
}