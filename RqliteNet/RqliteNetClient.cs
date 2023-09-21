using System.Text.Json;

namespace RqliteNet;

public class RqliteNetClient : IRqliteNetClient
{
    readonly HttpClient _http;
    static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public RqliteNetClient(string uri, HttpClient? client = null)
    {
        _http = client ?? new HttpClient() { BaseAddress = new Uri(uri) };
    }

    public async Task<ExecuteResponse> Execute(string command, params object[] parameters)
    {
        var json = $"[{ToQuery(command, parameters)}]";
        var response = await GetResponse("/db/execute", json);
        var obj = JsonSerializer.Deserialize<ExecuteResponse>(response, _options)!;

        if (obj.Results.Any() && !string.IsNullOrEmpty(obj.Results.First().Error))
        {
            throw new Exception(obj.Results.First().Error);
        }

        return obj;
    }

    public async Task<List<T>> Query<T>(string command, params object[] parameters) where T : new()
    {
        var json = $"[{ToQuery(command, parameters)}]";
        var response = await GetResponse("/db/query?pretty", json);

        var obj = JsonSerializer.Deserialize<QueryResponse>(response, _options);
        ArgumentNullException.ThrowIfNull(obj);

        if (obj.Results.Any() && !string.IsNullOrEmpty(obj.Results.First().Error))
        {
            throw new Exception(obj.Results.First().Error);
        }

        var list = new List<T>();

        foreach (var res in obj.Results.Where(x => x.Values is not null))
        {
            for (int i = 0; i < res.Values.Count; i++)
            {
                var dto = new T();

                foreach (var prop in typeof(T).GetProperties())
                {
                    var index = res.Columns.FindIndex(c => 0 == string.Compare(c, prop.Name, true));
                    var val = GetValue(res.Types[index], res.Values[i][index]);

                    prop.SetValue(dto, val);
                }

                list.Add(dto);
            }
        }

        return list;
    }

    static string ToQuery(string command, params object[] parameters)
    {
        var query = new List<object>
        {
            command
        };
        query.AddRange(parameters);

        return JsonSerializer.Serialize(query);
    }

    async Task<string> GetResponse(string url, string json)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json)
        };

        using var response = await _http.SendAsync(request);

        return await response.Content.ReadAsStringAsync();
    }

    static object GetValue(string valType, JsonElement el)
    {
        return valType.ToLowerInvariant() switch
        {
            "text" or "string" => el.GetString() ?? string.Empty,
            "integer" or "int" => el.GetInt32(),
            "boolean" => el.GetBoolean(),
            "real" or "double" or "float" or "double precision" => el.GetDouble(),
            "numeric" => el.GetDecimal(),

            _ => throw new ArgumentException($"Unsupported {valType}"),
        };
    }

    public void Dispose() => _http.Dispose();
}