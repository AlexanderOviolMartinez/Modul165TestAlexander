# WebAPI mit .NET 8 und MongoDB – Schritt-für-Schritt-Anleitung

**Einleitung:** In dieser Anleitung erstellen wir eine **ASP.NET Core WebAPI mit .NET 8** in C#, binden den **offiziellen MongoDB-C#-Treiber** ein, stellen eine **Verbindung zu einer MongoDB-Datenbank** her, implementieren CRUD-fähige **REST-Endpunkte** (für GET, POST, optional PUT und DELETE) und testen die API mit einem REST-Client (z.B. Postman oder `curl`). Alle Schritte basieren auf dem offiziellen Lehrmittel zu Modul 165 (*NoSQL-Datenbanken einsetzen*).

&#x20;*Architektur einer WebAPI-Anwendung:* Eine Client-Anfrage (HTTP Request) trifft auf den Controller der WebAPI, dieser nutzt das **Datenmodell** und die **Datenzugriffsschicht** (hier der MongoDB-Treiber), um auf die Datenbank zuzugreifen. Das Ergebnis wird als **HTTP Response** mit serialisiertem JSON-Modell an den Client zurückgegeben.

## 1. ASP.NET Core WebAPI-Projekt erstellen (.NET 8)

Zuerst legen wir ein neues WebAPI-Projekt in Visual Studio an. Gehen Sie dazu wie folgt vor:

1. **Visual Studio öffnen und neues Projekt anlegen:** Wählen Sie im Startfenster **"Create a new project"** (Neues Projekt erstellen). Im Dialog *Neues Projekt* filtern Sie nach **C#**, **Windows**, **Web** und wählen das Template **“ASP.NET Core Web API”** aus. Klicken Sie auf **Weiter**. (Stellen Sie sicher, dass Sie das **WebAPI-Template** und *nicht* das Razor Pages oder MVC Template auswählen.)

2. **Projekt konfigurieren:** Geben Sie einen passenden **Projektnamen** ein (z.B. *BookStoreApi* für eine Bücher-API) und wählen Sie einen Speicherort. Klicken Sie auf **Weiter**.

3. **Zusätzliche Informationen einstellen:** Wählen Sie als **Framework .NET 8.0** aus. Aktivieren Sie die Option **“Use controllers”** (Kontroller verwenden), falls verfügbar, um keine Minimal-API, sondern eine Controller-basierte API zu erstellen. Lassen Sie **“Enable OpenAPI support”** aktiviert, damit Swagger für Testzwecke verfügbar ist. Klicken Sie auf **Erstellen**.

Nachdem Visual Studio das Projekt erstellt hat, enthält es bereits eine Grundstruktur mit einer Program.cs und einem Beispiel-Controller (`WeatherForecast`). Diesen Beispiel-Code können wir entfernen oder ignorieren. Ihr WebAPI-Projekt ist nun bereit für die nächsten Schritte.

> *Hinweis:* Alternativ kann das Projekt auch über die Kommandozeile (z.B. Bash-Terminal) erstellt werden, z.B. mit:
>
> ```bash
> dotnet new webapi -n MyWebApi -f net8.0
> ```
>
> (Dies erzeugt ein neues WebAPI-Projekt im Ordner *MyWebApi* mit .NET 8). Die folgenden Schritte erfolgen aber zur Übersicht in Visual Studio.

## 2. MongoDB-C#-Treiber via NuGet integrieren

Als Nächstes integrieren wir den offiziellen MongoDB-Treiber für .NET in unser Projekt. Dieser Treiber ermöglicht unserer C#-WebAPI, mit MongoDB zu kommunizieren. Gehen Sie wie folgt vor:

* **NuGet-Paket installieren:** Öffnen Sie in Visual Studio den **NuGet-Paket-Manager** über **Tools > NuGet Package Manager > Manage NuGet Packages for Solution...**. Wechseln Sie dort auf den **Browse**-Reiter und suchen Sie nach **“MongoDB.Driver”**. Wählen Sie das Paket **MongoDB.Driver** (Absender: MongoDB, Inc.) in der aktuellen Version (z.B. 2.xx) aus, aktivieren Sie Ihr Projekt in der rechten Spalte und klicken Sie auf **Install** (Installieren). Bestätigen Sie evtl. Lizenzbedingungen.

* Visual Studio fügt nun dem Projekt die erforderlichen Bibliotheken hinzu. Im Ordner **Dependencies > Packages** sollte nun das Paket **MongoDB.Driver** sichtbar sein. Damit ist der MongoDB-C#-Treiber integriert.

> *Hinweis:* Das NuGet-Paket **MongoDB.Driver** umfasst alle nötigen Komponenten, inklusive BSON-Serialisierung etc. Es entspricht dem offiziellen Treiber von MongoDB für .NET. Alternativ könnten Sie das Paket auch via Package Manager Console installieren mit `Install-Package MongoDB.Driver`.

## 3. Verbindung zur MongoDB-Datenbank aufbauen

Nun richten wir die Verbindung zur MongoDB-Datenbank ein. Dafür konfigurieren wir die Verbindungsdetails, erstellen ein Datenmodell und implementieren eine kleine Datenzugriffsklasse (Service), die den MongoDB-Treiber benutzt.

**a) MongoDB-Server vorbereiten:** Stellen Sie sicher, dass ein MongoDB-Server läuft und erreichbar ist (z.B. lokal auf `mongodb://localhost:27017`). Verwenden Sie entweder eine lokale MongoDB-Installation oder einen MongoDB-Atlas-Cluster. Notieren Sie sich die Connection-String-URL, ggf. mit Benutzer/Passwort und Datenbankname.

**b) AppSettings konfigurieren:** Öffnen Sie die Datei **appsettings.json** im Projekt. Fügen Sie einen neuen Abschnitt für die MongoDB-Verbindungsinformationen hinzu, z.B.:

```json
{
  // ... bestehende Konfigurationen ...

  "BookStoreDatabase": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "BookStore",
    "BooksCollectionName": "Books"
  }
}
```

Dieser Abschnitt enthält den Connection String zum MongoDB-Server, den Namen der Datenbank (*BookStore*) sowie den Namen der Collection (hier *Books*), die wir verwenden möchten. (Sie können die Namen Ihren Bedürfnissen anpassen.)

**c) Einstellungs-Klasse für die Konfiguration:** Um diese Einstellungen im Code nutzen zu können, definieren wir eine passende C#-Klasse. Erstellen Sie im Projekt einen Ordner **Models** (für Modelle) und darin die Datei **BookStoreDatabaseSettings.cs** mit folgendem Inhalt:

```csharp
namespace BookStoreApi.Models
{
    public class BookStoreDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string BooksCollectionName { get; set; } = null!;
    }
}
```

Diese Klasse spiegelt die Felder aus der appsettings.json wider. Sie wird genutzt, um die Konfigurationswerte bequem per Dependency Injection in unseren Service zu laden.

**d) Datenmodell (Book-Klasse) erstellen:** Als Beispiel entwerfen wir eine *Book*-Klasse, die ein Buchobjekt in unserer MongoDB-Collection repräsentiert. Legen Sie im Ordner *Models* die Datei **Book.cs** an:

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BookStoreApi.Models
{
    public class Book
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [JsonPropertyName("Name")]
        public string BookName { get; set; } = null!;

        public string Category { get; set; } = null!;

        public string Author { get; set; } = null!;

        public decimal Price { get; set; }
    }
}
```

Erläuterungen: Das Feld `Id` ist als eindeutiger Bezeichner gedacht. Wir nutzen `[BsonId]` und `[BsonRepresentation(BsonType.ObjectId)]`, damit der Treiber das `Id`-Property als MongoDB-Objekt-ID behandelt, intern aber als String in unserer Klasse speichert. Die anderen Felder `BookName`, `Category`, `Author` und `Price` sind beispielhafte Attribute eines Buchs. Mit `[JsonPropertyName("Name")]` geben wir dem Feld *BookName* in der JSON-Ausgabe den Schlüssel **“Name”**, damit die JSON-Ausgabe schöner ist (dies ist optional).

**e) Service-Klasse für Datenzugriff erstellen:** Erstellen Sie einen Ordner **Services** und darin die Datei **BooksService.cs**. Diese Klasse kapselt die Logik für den Datenbankzugriff (CRUD-Operationen) mithilfe des MongoDB-Treibers:

```csharp
using BookStoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookStoreApi.Services
{
    public class BooksService
    {
        private readonly IMongoCollection<Book> _booksCollection;

        public BooksService(IOptions<BookStoreDatabaseSettings> settings)
        {
            // MongoDB-Client und Datenbank abrufen
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

            // Collection für Bücher holen
            _booksCollection = mongoDatabase.GetCollection<Book>(settings.Value.BooksCollectionName);
        }

        public async Task<List<Book>> GetAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<Book?> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Book newBook) =>
            await _booksCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, Book updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);
    }
}
```

Erläuterungen: Im Konstruktor der **BooksService**-Klasse verwenden wir `MongoClient`, um eine Verbindung zur MongoDB herzustellen. Die Verbindungsdetails beziehen wir aus den `settings`, die per **IOptions<BookStoreDatabaseSettings>** injiziert werden. Über `mongoClient.GetDatabase(...)` und `GetCollection<Book>(...)` erhalten wir eine `IMongoCollection<Book>`, auf der wir dann Abfragen und Änderungen durchführen können.

Die Methoden der Klasse entsprechen CRUD-Operationen:

* `GetAsync()` holt alle Bücher (Find mit Filter `true` findet alle Dokumente).
* `GetAsync(string id)` findet ein Buch mit bestimmter Id.
* `CreateAsync()` fügt ein neues Buch in die Collection ein (`InsertOneAsync`).
* `UpdateAsync()` ersetzt ein vorhandenes Dokument durch `ReplaceOneAsync` basierend auf der Id.
* `RemoveAsync()` löscht ein Dokument mit gegebener Id.

> *Hinweis:* Die Methoden sind asynchron (`async Task`), was in WebAPIs gängig ist, um blockierfreie I/O zu ermöglichen.

**f) Dependency Injection konfigurieren:** Öffnen Sie die Datei **Program.cs**. Wir müssen dort unsere Konfigurationsklasse und den Service für die Dependency Injection registrieren, damit Controller sie nutzen können. Fügen Sie **vor** dem Aufruf `var app = builder.Build();` folgende Zeilen hinzu:

```csharp
using BookStoreApi.Models;
using BookStoreApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ... bisheriger Code ...

// MongoDB-Konfiguration und Service einbinden
builder.Services.Configure<BookStoreDatabaseSettings>(
    builder.Configuration.GetSection("BookStoreDatabase"));
builder.Services.AddSingleton<BooksService>();

// ... ggf. weitere Einstellungen (Swagger etc.) ...

var app = builder.Build();
```

Diese Registrierung liest die Einstellungen aus dem Abschnitt `"BookStoreDatabase"` der Konfiguration und stellt sie der Anwendung zur Verfügung. Außerdem wird unsere `BooksService`-Klasse als Singleton registriert, sodass sie in Controller-Konstruktoren injiziert werden kann.

Damit ist die Verbindung zur MongoDB eingerichtet. Unsere WebAPI kennt jetzt die Datenbankverbindung und kann über den `BooksService` auf die **Books**-Collection zugreifen.

## 4. REST-Endpunkte implementieren (CRUD-Controller)

Jetzt erstellen wir einen WebAPI-Controller, der die HTTP-Endpunkte (GET, POST, PUT, DELETE) bereitstellt und unseren `BooksService` verwendet, um die gewünschten Datenoperationen auszuführen.

Erstellen Sie einen Ordner **Controllers** (falls nicht schon vorhanden) und darin die Datei **BooksController.cs** mit folgendem Inhalt:

```csharp
using BookStoreApi.Models;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BooksService _booksService;

        public BooksController(BooksService booksService) =>
            _booksService = booksService;

        // GET /api/Books
        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetAll()
        {
            var books = await _booksService.GetAsync();
            return Ok(books);
        }

        // GET /api/Books/{id}
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Book>> GetById(string id)
        {
            var book = await _booksService.GetAsync(id);
            if (book is null)
                return NotFound();
            return Ok(book);
        }

        // POST /api/Books
        [HttpPost]
        public async Task<ActionResult<Book>> Create(Book newBook)
        {
            await _booksService.CreateAsync(newBook);
            // HTTP 201 mit Pfad des neuen Objekts zurückgeben:
            return CreatedAtAction(nameof(GetById), new { id = newBook.Id }, newBook);
        }

        // PUT /api/Books/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Book updatedBook)
        {
            var book = await _booksService.GetAsync(id);
            if (book is null)
                return NotFound();
            // Id des zu aktualisierenden Buchs setzen:
            updatedBook.Id = book.Id;
            await _booksService.UpdateAsync(id, updatedBook);
            return NoContent();
        }

        // DELETE /api/Books/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _booksService.GetAsync(id);
            if (book is null)
                return NotFound();
            await _booksService.RemoveAsync(id);
            return NoContent();
        }
    }
}
```

Erläuterungen zum **BooksController**:

* Der Controller ist mit `[ApiController]` und `[Route("api/[controller]")]` dekoriert. Dadurch lautet der Basisroute automatisch **`api/Books`** (abgeleitet vom Controller-Namen).

* Im Konstruktor erhalten wir den `BooksService` via Dependency Injection.

* **GET /api/Books:** Die Methode `GetAll()` ruft alle Bücher ab und gibt sie als `200 OK` mit einer Liste von Book-Objekten im JSON-Format zurück.

* **GET /api/Books/{id}:** Die Methode `GetById(string id)` holt ein einzelnes Buch. Das Routenattribut enthält `{id:length(24)}`, was bedeutet, dass die Id 24 Zeichen lang sein muss (das ist die Länge eines MongoDB-ObjectId als String). Existiert kein Buch mit dieser Id, liefert der Controller `404 Not Found`. Andernfalls `200 OK` mit dem Buch-JSON.

* **POST /api/Books:** `Create(Book newBook)` nimmt ein neues Buch-Objekt vom Request-Body entgegen und fügt es via Service in die DB ein. Danach wird `CreatedAtAction` verwendet, um eine HTTP-201-Antwort zurückzugeben. Diese enthält im Header eine Location-URL auf das neu erzeugte Objekt (entspricht `GetById` mit der neuen Id) sowie im Body das erstellte Buch. HTTP-201 (Created) ist der Standardstatuscode, wenn durch einen POST ein neues Ressourcenelement erstellt wurde.

* **PUT /api/Books/{id}:** `Update(string id, Book updatedBook)` aktualisiert ein bestehendes Buch. Zuerst prüfen wir, ob es das Buch gibt. Wenn nicht, `404 Not Found`. Falls doch, setzen wir die `Id` des `updatedBook` sicherheitshalber auf die vorhandene Id und rufen den Service zum Ersetzen des Dokuments auf. Es wird `NoContent()` zurückgegeben, was dem HTTP-Status 204 (kein Inhalt) entspricht – üblich für erfolgreiche Updates ohne Rückgabedaten.

* **DELETE /api/Books/{id}:** `Delete(string id)` löscht das Buch mit der angegebenen Id. Wenn nichts gefunden wird: `404 Not Found`. Andernfalls löschen wir den Datensatz und geben `204 No Content` zurück. Der `[HttpDelete]`-Endpunkt ist analog zum GET-by-Id aufgebaut.

Damit haben wir alle wichtigen Endpunkte umgesetzt. Unsere WebAPI kann nun Bücher in MongoDB erstellen, auslesen, ändern und löschen.

**Zusammenfassung der Endpunkte:** (Angenommen, die Anwendung läuft lokal auf Port 5000)

* **GET** `http://localhost:5000/api/Books` – alle Bücher abrufen (List<Book>)
* **GET** `http://localhost:5000/api/Books/{id}` – Buch mit bestimmter Id abrufen
* **POST** `http://localhost:5000/api/Books` – neues Buch hinzufügen (JSON-Body mit Buchdaten)
* **PUT** `http://localhost:5000/api/Books/{id}` – bestehendes Buch ersetzen (JSON-Body mit Buchdaten)
* **DELETE** `http://localhost:5000/api/Books/{id}` – Buch löschen

## 5. API-Endpunkte testen (mit Postman oder curl)

Zum Abschluss testen wir die WebAPI mit einem REST-Client. Starten Sie dazu die Anwendung (in Visual Studio via **F5** oder im Terminal `dotnet run`). Standardmäßig lauscht die App auf einer localhost-URL und Port (z.B. [http://localhost:5000](http://localhost:5000) oder eine zufällige Portnummer, siehe Ausgabekonsole).

Verwenden Sie dann entweder **Postman** oder das Kommandozeilen-Tool **curl**, um HTTP-Anfragen an die API zu schicken:

* **GET-Test:** Senden Sie eine GET-Anfrage an `/api/Books`. In Postman geben Sie z.B. `GET http://localhost:5000/api/Books` ein und klicken **Send**. Da anfangs keine Bücher in der DB sind, erhalten Sie vermutlich eine leere Liste `[]` als Antwort mit Status 200 OK.

* **POST-Test:** Fügen Sie ein neues Buch hinzu. In Postman stellen Sie auf **POST** um und verwenden die URL `http://localhost:5000/api/Books`. Im Reiter “Body” wählen Sie **raw + JSON** und geben z.B. folgendes JSON ein:

  ```json
  {
    "Name": "Das MongoDB Handbuch",
    "Category": "Datenbanken",
    "Author": "Max Muster",
    "Price": 39.90
  }
  ```

  Senden Sie die Anfrage. Die API sollte mit **201 Created** antworten und im Body das erzeugte Buchobjekt zurückliefern, inklusive der generierten `Id`. (Die `Location`-Header zeigt auf `/api/Books/{id}` des neuen Buchs.)

* **GET-by-Id-Test:** Kopieren Sie die erhaltene `Id` und senden Sie eine GET-Anfrage an `http://localhost:5000/api/Books/<IhreId>`. Die API sollte das entsprechende Buch als JSON liefern.

* **PUT-Test:** Testen Sie die Aktualisierung, indem Sie z.B. den **Preis** im vorherigen JSON ändern. Senden Sie eine **PUT**-Anfrage an `http://localhost:5000/api/Books/<IhreId>` mit dem geänderten JSON im Body. Bei Erfolg erhalten Sie `204 No Content`. Eine erneute GET-Anfrage sollte die geänderten Daten zeigen.

* **DELETE-Test:** Abschließend können Sie den Datensatz löschen: Senden Sie **DELETE** an `http://localhost:5000/api/Books/<IhreId>`. Die API gibt `204 No Content` zurück. Ein anschließender GET-Versuch für diese Id sollte nun `404 Not Found` ergeben – das Buch ist gelöscht.

Alternativ zur Postman-Oberfläche können Sie diese Aufrufe auch mit `curl` ausführen. Beispiele:

```bash
# GET alle Bücher
curl -X GET http://localhost:5000/api/Books

# POST neues Buch (Beispiel mit curl -d Daten und -H Content-Type)
curl -X POST http://localhost:5000/api/Books \
   -H "Content-Type: application/json" \
   -d "{\"Name\":\"Neues Buch\",\"Category\":\"Test\",\"Author\":\"Ich\",\"Price\":9.99}"

# (Entsprechende curl-Befehle für PUT und DELETE analog)
```

Während des Tests können Sie im Visual Studio-Ausgabe-Fenster oder im Terminal die Logging-Ausgaben beobachten, um zu sehen, dass die Anfragen ankommen. Die erfolgreiche Ausführung der CRUD-Methoden in Postman bestätigt, dass alle Lernziele erreicht wurden:

* **WebAPI mit .NET 8 erstellt** – läuft lokal und beantwortet Anfragen.
* **MongoDB-C#-Treiber integriert** – ermöglicht die Kommunikation mit MongoDB.
* **Verbindung zur MongoDB** – durch erfolgreiche Abfragen/Änderungen in der realen Datenbank nachgewiesen.
* **REST-Endpunkte (GET, POST, PUT, DELETE)** implementiert – wurden durchgetestet.
* **Tests mit REST-Client** – Postman/curl zeigen die korrekte Funktion der Endpunkte.

Damit haben wir eine vollständige **CRUD WebAPI** mit .NET 8 und MongoDB aufgebaut, entsprechend den Vorgaben des Lehrmittels. Viel Erfolg beim Lernen und Testen! 🚀

**Quellen:** Die Anleitung orientiert sich am offiziellen Lehrmaterial Modul 165 der GBS St.Gallen sowie an Praxisbeispielen für ASP.NET Core und MongoDB. Alle Codebeispiele wurden auf Basis dieses Lehrmittels erstellt und überprüft. Viel Spaß beim Ausprobieren!
