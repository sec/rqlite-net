using RqliteNet;

using IRqliteNetClient r = new RqliteNetClient("http://127.0.0.1:4001");

await r.Execute("DROP TABLE IF EXISTS TEST");
await r.Execute("CREATE TABLE IF NOT EXISTS TEST (Id INT, Val TEXT)");
await r.Execute("INSERT INTO TEST (Id, Val) VALUES (?, ?)", 123, "Hello World!");

var result = await r.Query<TestDto>("SELECT * FROM TEST WHERE Id = ?", 123);
var dto = result.First();
Console.WriteLine($"{dto.Id} said {dto.Val}");

await r.Execute("DROP TABLE IF EXISTS TEST");

internal class TestDto
{
    public int Id { get; set; }
    public string? Val { get; set; }
}
