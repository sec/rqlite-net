using RqliteNet;
using RqliteNet.AspNet;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRqliteNet(opt =>
{
    opt.Uri = "http://127.0.0.1:4001";
});

var app = builder.Build();
app.MapGet("/hello", async (IRqliteNetClient r) =>
{
    var number = Random.Shared.Next(1, 1000);

    await r.Execute("CREATE TABLE IF NOT EXISTS TEST (Id INTEGER, Val TEXT)");
    await r.Execute("INSERT INTO TEST (Id, Val) VALUES (?, ?)", number, "Hello World!");

    var result = await r.Query<TestDto>("SELECT * FROM TEST WHERE Id = ?", number);
    var dto = result.First();

    return $"{dto.Id} said {dto.Val}";
});

app.Run();

internal class TestDto
{
    public int Id { get; set; }
    public string? Val { get; set; }
}