## 🧩 **Lernziel 1: MongoDB-Treiber in .NET 8 integrieren**

---

### 🔹 **Schritt 1: Neues .NET 8 WebAPI-Projekt erstellen**

Falls noch nicht geschehen, erstelle ein neues Projekt mit dem Befehl:

```bash
dotnet new webapi -n MongoWebApiDemo
cd MongoWebApiDemo
```

> Du kannst optional `--no-https` anhängen, falls du ohne HTTPS testen willst.

---

### 🔹 **Schritt 2: MongoDB C# Treiber installieren**

Installiere den offiziellen MongoDB-Treiber mit dem NuGet-Paketmanager:

```bash
dotnet add package MongoDB.Driver
```

Alternativ über die `.csproj` Datei:

```xml
<PackageReference Include="MongoDB.Driver" Version="2.20.0" />
```

---

### 🔹 **Schritt 3: MongoDB-Konfigurationsdatei anpassen (appsettings.json)**

In der Datei `appsettings.json` fügst du deine Verbindungsdaten hinzu:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MeineDatenbank"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> Passe `localhost:27017` und `MeineDatenbank` ggf. an deine MongoDB-Umgebung an.

---

### 🔹 **Schritt 4: Eine Klasse zur Konfiguration erstellen**

Erstelle im Projektordner z. B. eine Datei `MongoDbSettings.cs`:

```csharp
namespace MongoWebApiDemo;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}
```

---

### 🔹 **Schritt 5: Konfiguration in `Program.cs` einbinden**

Füge folgende Zeilen in `Program.cs` ein, um die Konfiguration zu laden:

```csharp
using MongoWebApiDemo;

var builder = WebApplication.CreateBuilder(args);

// MongoDB-Einstellungen binden
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Anwendung starten
var app = builder.Build();
app.Run();
```

Optional kannst du auch die Settings direkt abrufen:

```csharp
var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
Console.WriteLine($"Verbindung zu MongoDB: {settings.ConnectionString}");
```

---

### ✅ **Ergebnis dieses Lernziels**

* Du hast erfolgreich den **MongoDB-Treiber integriert**.
* Du hast eine **saubere Konfigurationsstruktur** für MongoDB.
* Deine Anwendung kann jetzt **mit MongoDB kommunizieren** (bereit für CRUD-Operationen).


---

## 🧩 **Lernziel 2: Verbindung zur MongoDB aufbauen**

---

### 🔹 **Ziel dieses Schritts**

* Eine Verbindung zur MongoDB-Datenbank wird hergestellt
* Du bekommst Zugriff auf eine **Collection** (z. B. `Books`, `Products`, etc.)
* Das Ganze wird als **Service** oder Repository eingerichtet

---

### 🔹 **Schritt 1: MongoDB-Client einrichten**

Erstelle eine neue Klasse z. B. `MongoDbService.cs`:

```csharp
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoWebApiDemo;

public class MongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}
```

---

### 🔹 **Schritt 2: Service in DI-Container eintragen**

In deiner `Program.cs`:

```csharp
builder.Services.AddSingleton<MongoDbService>();
```

Die komplette `Program.cs` sieht jetzt z. B. so aus:

```csharp
using MongoWebApiDemo;

var builder = WebApplication.CreateBuilder(args);

// MongoDB-Konfiguration laden
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// MongoDB-Dienst registrieren
builder.Services.AddSingleton<MongoDbService>();

// Controller aktivieren (wenn nötig)
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers(); // für API-Endpunkte

app.Run();
```

---

### 🔹 **Schritt 3: Test: Verbindung prüfen**

Erstelle z. B. einen Controller `TestController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace MongoWebApiDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;

    public TestController(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpGet]
    public IActionResult TestVerbindung()
    {
        try
        {
            var db = _mongoDbService.GetCollection<BsonDocument>("TestCollection");
            return Ok("MongoDB-Verbindung funktioniert!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fehler: {ex.Message}");
        }
    }
}
```

---

### 🔹 **Schritt 4: Aufrufen & testen**

Starte die Anwendung:

```bash
dotnet run
```

Rufe im Browser oder mit einem REST-Client auf:

```
GET http://localhost:5000/api/test
```

> Wenn alles funktioniert, bekommst du:
> ✅ **"MongoDB-Verbindung funktioniert!"**

---

### ✅ **Ergebnis dieses Lernziels**

* Du hast eine funktionierende Verbindung zur MongoDB.
* Du kannst auf Collections zugreifen (`GetCollection<T>()`).
* Die Konfiguration ist in einer sauberen Architektur gekapselt.

---
---

## 🧩 **Lernziel 3: REST-Endpunkte für MongoDB erstellen**

Wir bauen eine einfache API für das Beispiel-Modell `Book` mit den Endpunkten:

* `GET /api/books` → alle Bücher lesen
* `POST /api/books` → neues Buch speichern

---

### 🔹 **Schritt 1: Modellklasse erstellen**

Datei: `Models/Book.cs`

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoWebApiDemo.Models;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("author")]
    public string Author { get; set; } = string.Empty;

    [BsonElement("year")]
    public int Year { get; set; }
}
```

---

### 🔹 **Schritt 2: Serviceklasse für Datenzugriff erstellen**

Datei: `Services/BookService.cs`

```csharp
using MongoDB.Driver;
using MongoWebApiDemo.Models;

namespace MongoWebApiDemo.Services;

public class BookService
{
    private readonly IMongoCollection<Book> _books;

    public BookService(MongoDbService mongoService)
    {
        _books = mongoService.GetCollection<Book>("Books");
    }

    public async Task<List<Book>> GetAllAsync() =>
        await _books.Find(_ => true).ToListAsync();

    public async Task AddAsync(Book book) =>
        await _books.InsertOneAsync(book);
}
```

---

### 🔹 **Schritt 3: BookService registrieren**

In `Program.cs` hinzufügen:

```csharp
builder.Services.AddSingleton<BookService>();
```

---

### 🔹 **Schritt 4: REST-Controller erstellen**

Datei: `Controllers/BooksController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using MongoWebApiDemo.Models;
using MongoWebApiDemo.Services;

namespace MongoWebApiDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;

    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> Get()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Book book)
    {
        await _bookService.AddAsync(book);
        return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
    }
}
```

---

### 🔹 **Schritt 5: Testen mit REST-Client**

Beispiel mit **Postman** oder **Thunder Client**:

* **GET** `http://localhost:5000/api/books`
  ➝ Gibt alle Bücher zurück

* **POST** `http://localhost:5000/api/books`
  Body (JSON):

  ```json
  {
    "title": "1984",
    "author": "George Orwell",
    "year": 1949
  }
  ```

---

### ✅ **Ergebnis dieses Lernziels**

* Du hast erfolgreich **REST-Endpunkte** für `GET` und `POST` umgesetzt.
* Die API ist mit MongoDB verbunden.
* Du kannst Daten **einfügen und abrufen**.

---


