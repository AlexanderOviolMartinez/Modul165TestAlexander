using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// SongService als Singleton registrieren
builder.Services.AddSingleton<ISongService>(sp =>
{
    var connectionString = builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString");
    return new SongService(connectionString);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/databases", (ISongService songService) =>
{
    try
    {
        var result = songService.GetDatabases();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem("Fehler: " + ex.Message);
    }
});

app.MapPost("/api/songs", (Song song) =>
{
    throw new NotImplementedException();
});

app.MapGet("/api/songs", () =>
{
    throw new NotImplementedException();
});

app.MapGet("/api/songs/{id}", (string id) =>
{
    throw new NotImplementedException();
});

app.MapPut("/api/songs/{id}", (string id, Song song) =>
{
    throw new NotImplementedException();
});

app.MapDelete("/api/songs/{id}", (string id) =>
{
    throw new NotImplementedException();
});

app.Run();