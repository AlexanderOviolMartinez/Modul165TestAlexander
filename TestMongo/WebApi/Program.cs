using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var movieDatabaseConfigSection = builder.Configuration.GetSection("DatabaseSettings");
builder.Services.Configure<DatabaseSettings>(movieDatabaseConfigSection);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/check", (Microsoft.Extensions.Options.IOptions<DatabaseSettings> options) =>
{
    try
    {
        var mongoDbConnectionString = options.Value.ConnectionString;
        var mongoClient = new MongoClient(mongoDbConnectionString);
        var databaseName = mongoClient.ListDatabaseNames().ToList();
        return $"Zugriff auf MongoDB ok. Vorhandene DBs: {String.Join(", ", databaseName)}";
    }
    catch (System.Exception ex)
    {
        return ex.Message;
    }
});


// Insert Movie
// Wenn das übergebene Objekt eingefügt werden konnte,
// wird es mit Statuscode 200 zurückgegeben.
// Bei Fehler wird Statuscode 409 Conflict zurückgegeben.
app.MapPost("/api/movies", (Movie movie) =>
{
    throw new NotImplementedException();
});

// Get all Movies
// Gibt alle vorhandenen Movie-Objekte mit Statuscode 200 OK zurück.
app.MapGet("/api/movies", () =>
{
    throw new NotImplementedException();
});


// Get Movie by id
// Gibt das gewünschte Movie-Objekt mit Statuscode 200 OK zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapGet("/api/movies/{id}", (string id) =>
{
    if (id == "1")
    {
        var myMovie = new Movie()
        {
            Id = "1",
            Title = "Ein Quantum Trost",
        };
        return Results.Ok(myMovie);
    }
    else
    {
        return Results.NotFound();
    }
});



// Update Movie
// Gibt das aktualisierte Movie-Objekt zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapPut("/api/movies/{id}", (string id, Movie movie) =>
{
    throw new NotImplementedException();
});
// Delete Movie
// Gibt bei erfolgreicher Löschung Statuscode 200 OK zurück.7.43 Minimal API mit MongoDB  Teil 32
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapDelete("/api/movies/{id}", (string id) =>
{
    throw new NotImplementedException();
});


app.Run();
