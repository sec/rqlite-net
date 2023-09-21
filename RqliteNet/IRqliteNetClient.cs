namespace RqliteNet;

public interface IRqliteNetClient : IDisposable
{
    Task<ExecuteResponse> Execute(string command, params object[] parameters);

    Task<List<T>> Query<T>(string command, params object[] parameters) where T : new();
}