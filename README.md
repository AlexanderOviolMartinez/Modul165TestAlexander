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


