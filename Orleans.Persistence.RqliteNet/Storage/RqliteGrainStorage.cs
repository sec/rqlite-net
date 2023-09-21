using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Persistence.RqliteNet.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using RqliteNet;

namespace Orleans.Persistence.RqliteNet.Storage;

public class RqliteGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _storageName;
    private readonly RqliteGrainStorageOptions _options;
    private readonly ClusterOptions _clusterOptions;
    private readonly IRqliteNetClient _client;

    public RqliteGrainStorage(string storageName, RqliteGrainStorageOptions options, IOptions<ClusterOptions> clusterOptions)
    {
        _storageName = storageName;
        _options = options;
        _clusterOptions = clusterOptions.Value;
        _client = new RqliteNetClient(_options.Uri);
    }

    private string GetKeyString(string stateName, GrainId grainId) => $"{_clusterOptions.ServiceId}.{grainId}.{stateName}";

    private Task<List<StateDto>> GetCustomState(string id)
    {
        return _client.Query<StateDto>("SELECT * FROM GrainsState WHERE GrainId = ?", id);
    }

    public void Participate(ISiloLifecycle observer)
    {
        observer.Subscribe(
            observerName: OptionFormattingUtilities.Name<RqliteGrainStorageOptions>(_storageName),
            stage: ServiceLifecycleStage.ApplicationServices,
            onStart: async (ct) =>
            {
                var queryResults = await _client.Execute("CREATE TABLE IF NOT EXISTS GrainsState (GrainId TEXT PRIMARY KEY, ETag TEXT, State TEXT)");
            });
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var id = GetKeyString(stateName, grainId);
        var result = await GetCustomState(id);
        if (result.Any())
        {
            if (result.Single().ETag != grainState.ETag)
            {
                throw new InconsistentStateException("ETag mismatch.");
            }
            await _client.Execute($"DELETE FROM GrainsState WHERE GrainId=?", id);

            grainState.ETag = null;
            grainState.State = (T) Activator.CreateInstance(typeof(T))!;
            grainState.RecordExists = false;
        }
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var id = GetKeyString(stateName, grainId);
        var result = await GetCustomState(id);
        if (!result.Any())
        {
            grainState.State = (T) Activator.CreateInstance(typeof(T))!;
            return;
        }

        var state = result.Single();
        var bytes = Convert.FromBase64String(state.State);

        grainState.State = _options.GrainStorageSerializer.Deserialize<T>(new BinaryData(bytes));
        grainState.ETag = state.ETag;
        grainState.RecordExists = true;
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storedData = _options.GrainStorageSerializer.Serialize(grainState.State);
        var dataToSave = Convert.ToBase64String(storedData.ToArray());

        var id = GetKeyString(stateName, grainId);
        var result = await GetCustomState(id);
        if (result.Any())
        {
            if (result.Single().ETag != grainState.ETag)
            {
                throw new InconsistentStateException("ETag mismatch.");
            }
            await _client.Execute($"DELETE FROM GrainsState WHERE GrainId=?", id);
        }

        grainState.ETag = Guid.NewGuid().ToString();

        var insertResult = await _client.Execute($"INSERT INTO GrainsState (GrainId, ETag, State) VALUES (?, ?, ?)", id, grainState.ETag, dataToSave);
        if (insertResult.Results!.Single().RowsAffected != 1)
        {
            throw new InconsistentStateException("Error during row insert.");
        }

        grainState.RecordExists = true;
    }
}