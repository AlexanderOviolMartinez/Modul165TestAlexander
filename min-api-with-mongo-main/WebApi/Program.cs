using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

var movieDatabaseConfigSection = builder.Configuration.GetSection("DatabaseSettings");
builder.Services.Configure<DatabaseSettings>(movieDatabaseConfigSection);
builder.Services.AddSingleton<IMovieService, MongoMovieService>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

// docker run --name mongodb -d -p 27017:27017 -v data:/data/db -e MONGO_INITDB_ROOT_USERNAME=gbs -e MONGO_INITDB_ROOT_PASSWORD=geheim mongo
app.MapGet("/check", (Microsoft.Extensions.Options.IOptions<DatabaseSettings> options) =>
{
    try
    {
        // MongoDB Connection-String (ggf. anpassen)
        var connectionString = options.Value.ConnectionString;

        // Verbindung aufbauen
        var client = new MongoClient(connectionString);

        // Liste der Datenbanken abrufen
        var databases = client.ListDatabaseNames().ToList();

        // Erfolg: Datenbanken als String zurückgeben
        return Results.Ok(new
        {
            Message = "Zugriff auf MongoDB ok.",
            Databases = databases
        });
    }
    catch (Exception ex)
    {
        // Fehlerbehandlung
        return Results.Problem($"Fehler beim Zugriff auf MongoDB: {ex.Message}");
    }
});

// Insert Movie
// Wenn das übergebene Objekt eingefügt werden konnte,
// wird es mit Statuscode 200 zurückgegeben.
// Bei Fehler wird Statuscode 409 Conflict zurückgegeben.
app.MapPost("/api/movies", (Movie movie, IMovieService movieService) =>
{
    movieService.Create(movie);
    return Results.Ok(movie);
});
// Get all Movies
// Gibt alle vorhandenen Movie-Objekte mit Statuscode 200 OK zurück.
app.MapGet("/api/movies", (IMovieService movieService) =>
{
    var movies = movieService.Get();
    return Results.Ok(movies);
});
// Get Movie by id
// Gibt das gewünschte Movie-Objekt mit Statuscode 200 OK zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapGet("/api/movies/{id}", (IMovieService movieService, string id) =>
{
    var movie = movieService.Get(id);
    return movie != null
    ? Results.Ok(movie)
    : Results.NotFound();
});
// Update Movie
// Gibt das aktualisierte Movie-Objekt zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapPut("/api/movies/{id}", (string id, IMovieService movieService, Movie movie) =>
{
    var existingMovie = movieService.Get(id);
    if (existingMovie == null)
    {
        return Results.NotFound();
    }

    movieService.Update(id, movie);
    return Results.Ok(movie);
});
// Delete Movie
// Gibt bei erfolgreicher Löschung Statuscode 200 OK zurück.
// Bei ungültiger id wird Statuscode 404 not found zurückgegeben.
app.MapDelete("/api/movies/{id}", (string id,IMovieService movieService) =>
{
    var movie = movieService.Get(id);
    if (movie is null)
    {
        return Results.NotFound();
    }

    movieService.Delete(id);
    return Results.Ok();
});


app.Run();
