# Udvidet Øvelse: Microservices med YARP og C#

I denne øvelse bygger du et microservice-setup med tre .NET Web API'er og en API Gateway med YARP. Hver service kører som sin egen Docker-container, og gatewayen håndterer routing og adgangskontrol.

## Mål
- Forstå microservice-arkitektur i praksis
- Oprette og deploye tre .NET-services
- Konfigurere YARP til routing og sikkerhed
- Orkestrere hele miljøet med Docker Compose

---

## Del 1: Projektstruktur
**📁 Formål:** Få overblik over mappestruktur og filplacering

```
/MicroserviceDemo
├── Gateway
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Gateway.csproj
│   └── Dockerfile
├── KundeService
│   ├── Controllers/KundeController.cs
│   ├── Program.cs
│   ├── KundeService.csproj
│   └── Dockerfile
├── ProduktService
│   ├── Controllers/ProduktController.cs
│   ├── Program.cs
│   ├── ProduktService.csproj
│   └── Dockerfile
├── OrdreService
│   ├── Controllers/OrdreController.cs
│   ├── Program.cs
│   ├── OrdreService.csproj
│   └── Dockerfile
└── docker-compose.yml
```

---

## Del 2: Opret KundeService

💬 **Refleksion:**
- Hvilken værdi giver det at isolere en kundeservice som sin egen API?
- Hvordan kunne man beskytte dette endpoint mod misbrug (f.eks. rate limiting, auth)?
**👤 Formål:** Skabe en enkel C#-baseret API for kundeoplysninger

### `KundeController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class KundeController : ControllerBase
{
    private readonly KundeContext _context;

    public KundeController(KundeContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var kunder = await _context.Kunder.ToListAsync();
        return Ok(kunder);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Kunde nyKunde)
    {
        _context.Kunder.Add(nyKunde);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = nyKunde.Id }, nyKunde);
    }
}
```

### `Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapControllers();
app.Run();
```

### `KundeService.csproj`
Sørg for at filen inkluderer ASP.NET-pakken:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### `Dockerfile`
```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KundeService.dll"]
```


---

## Del 3: Opret ProduktService

💬 **Refleksion:**
- Skal alle have adgang til produktoplysninger, eller skal de også sikres?
- Hvad sker der, hvis produktdata ændres samtidig af to brugere?
**📦 Formål:** Tilføj en ny microservice til produkter

### `ProduktController.cs`
```csharp
[ApiController]
[Route("[controller]")]
public class ProduktController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { navn = "Produkt A", pris = 123 });
}
```

### `Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapControllers();
app.Run();
```

### `ProduktService.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### `Dockerfile`
```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProduktService.dll"]
```

- Struktur og opbygning er magen til KundeService
- Returnér fx `{ navn: "Produkt A", pris: 123 }`

---

## Del 4: Opret OrdreService

💬 **Refleksion:**
- Hvilke forretningsregler skal gælde for ordrer? Kan alle oprette en?
- Hvordan kan man sikre dataens validitet (f.eks. produkt-ID findes)?
**🧾 Formål:** Introducér en tredje service med simple ordredata

### `OrdreController.cs`
```csharp
[ApiController]
[Route("[controller]")]
public class OrdreController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { ordreId = 1, kunde = "Test Kunde" });
}
```

### `Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapControllers();
app.Run();
```

### `OrdreService.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### `Dockerfile`
```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrdreService.dll"]
```

- Samme setup som de andre
- Controller returnerer fx `{ ordreId: 1, kunde: "Test Kunde" }`

---

## Del 5: Opret Gateway med YARP

💬 **Refleksion:**
- Hvad er fordelene ved at samle auth og routing i en gateway?
- Hvad sker der, hvis en bruger får adgang til gatewayen uden korrekt token?
**🧭 Formål:** Brug YARP til at rute trafik til de tre services

### `Program.cs`
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Token-baseret auth
builder.Services.AddAuthentication("MyScheme")
    .AddJwtBearer("MyScheme", options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"];
                if (token == "Bearer demo-token")
                {
                    context.Principal = new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity("MyScheme")
                    );
                    context.Success();
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy().RequireAuthorization();

app.Run("http://0.0.0.0:80");
```

### `appsettings.json`
```json
{
  "ReverseProxy": {
    "Routes": {
      "kunde": {
        "ClusterId": "kunde-cluster",
        "Match": { "Path": "/kunde/{**catch-all}" },
        "Transforms": [{ "PathRemovePrefix": "/kunde" }]
      },
      "produkt": {
        "ClusterId": "produkt-cluster",
        "Match": { "Path": "/produkt/{**catch-all}" },
        "Transforms": [{ "PathRemovePrefix": "/produkt" }]
      },
      "ordre": {
        "ClusterId": "ordre-cluster",
        "Match": { "Path": "/ordre/{**catch-all}" },
        "Transforms": [{ "PathRemovePrefix": "/ordre" }]
      }
    },
    "Clusters": {
      "kunde-cluster": {
        "Destinations": {
          "dest1": { "Address": "http://kunde:80/" }
        }
      },
      "produkt-cluster": {
        "Destinations": {
          "dest1": { "Address": "http://produkt:80/" }
        }
      },
      "ordre-cluster": {
        "Destinations": {
          "dest1": { "Address": "http://ordre:80/" }
        }
      }
    }
  }
}
```

### `Gateway.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Yarp.ReverseProxy" Version="2.3.0" />
  </ItemGroup>
</Project>
```

### `Dockerfile`
```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]
```


"Routes": {
  "kunde": {
    "ClusterId": "kunde-cluster",
    "Match": { "Path": "/kunde/{**catch-all}" },
    "Transforms": [{ "PathRemovePrefix": "/kunde" }]
  },
  "produkt": {
    "ClusterId": "produkt-cluster",
    "Match": { "Path": "/produkt/{**catch-all}" },
    "Transforms": [{ "PathRemovePrefix": "/produkt" }]
  },
  "ordre": {
    "ClusterId": "ordre-cluster",
    "Match": { "Path": "/ordre/{**catch-all}" },
    "Transforms": [{ "PathRemovePrefix": "/ordre" }]
  }
}
```

---

## Del 6: Saml det hele med Docker Compose

💬 **Refleksion:**
- Hvordan hjælper Docker Compose med at orkestrere microservices?
- Hvad sker der, hvis en service crasher – hvordan kan vi opdage og håndtere det?
**🐋 Formål:** Start alle services med én kommando

Eksempel på `docker-compose.yml`:
```yaml
version: '3.9'
services:
  kunde:
    build: ./KundeService
    ports: ["6001:80"]

  produkt:
    build: ./ProduktService
    ports: ["6002:80"]

  ordre:
    build: ./OrdreService
    ports: ["6003:80"]

  gateway:
    build: ./Gateway
    ports: ["8000:80"]
    depends_on:
      - kunde
      - produkt
      - ordre
```

---

## Del 7: Test med Postman eller curl

💬 **Refleksion:**
- Hvilke endpoints virker uden token, og hvilke fejler?
- Hvordan kan du se, om gatewayen faktisk håndterer autorisationen korrekt?
**🧪 Formål:** Verificér at alt virker via gatewayen

Test med:
```bash
curl -H "Authorization: Bearer demo-token" http://localhost:8000/kunde
curl -H "Authorization: Bearer demo-token" http://localhost:8000/produkt
curl -H "Authorization: Bearer demo-token" http://localhost:8000/ordre
```

---

## Del 8: Tilføj Swagger UI til alle services

💬 **Refleksion:**
- Hvordan hjælper Swagger udviklere og testere?
- Er det en sikkerhedstrussel at eksponere Swagger offentligt?
**📚 Formål:** Gør det muligt at se og teste API-endpoints direkte i browseren via en auto-genereret UI

### I hver services `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

➡️ Gå derefter til fx:
```
http://localhost:6001/swagger
```

Her kan du teste dine endpoints direkte i browseren.

---

## Del 9: Tilføj MySQL-database til dine services

💬 **Refleksion:**
- Hvorfor er det vigtigt at isolere databaser pr. service?
- Hvordan kunne man validere input før det gemmes i databasen?

### Automatisk databaseinitiering og seed-data i ProduktService

**Formål:** Opretter tabeller og lægger demo-produkter ind ved opstart.

- Tilføj EF Core og Pomelo:
```bash
cd ProduktService

dotnet add package Microsoft.EntityFrameworkCore

dotnet add package Pomelo.EntityFrameworkCore.MySql
```

- Tilføj model og context:
```csharp
// Models/Produkt.cs
public class Produkt {
    public int Id { get; set; }
    public string Navn { get; set; }
    public decimal Pris { get; set; }
}

// Data/ProduktContext.cs
public class ProduktContext : DbContext {
    public ProduktContext(DbContextOptions<ProduktContext> options) : base(options) {}
    public DbSet<Produkt> Produkter => Set<Produkt>();
}
```

- I `Program.cs`:
```csharp
builder.Services.AddDbContext<ProduktContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProduktContext>();
    db.Database.Migrate();

    if (!db.Produkter.Any())
    {
        db.Produkter.AddRange(
            new Produkt { Navn = "Blyant", Pris = 5.95m },
            new Produkt { Navn = "Papir", Pris = 12.50m }
        );
        db.SaveChanges();
    }
}
```

- `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=mysql;port=3306;database=produktdb;user=user;password=password"
  }
}
```

---

### Automatisk databaseinitiering og seed-data i OrdreService

**Formål:** Opretter tabeller og demo-ordrer ved opstart.

- Tilføj EF Core og Pomelo:
```bash
cd OrdreService

dotnet add package Microsoft.EntityFrameworkCore

dotnet add package Pomelo.EntityFrameworkCore.MySql
```

- Model og context:
```csharp
// Models/Ordre.cs
public class Ordre {
    public int Id { get; set; }
    public string KundeNavn { get; set; }
    public DateTime Dato { get; set; }
}

// Data/OrdreContext.cs
public class OrdreContext : DbContext {
    public OrdreContext(DbContextOptions<OrdreContext> options) : base(options) {}
    public DbSet<Ordre> Ordrer => Set<Ordre>();
}
```

- I `Program.cs`:
```csharp
builder.Services.AddDbContext<OrdreContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdreContext>();
    db.Database.Migrate();

    if (!db.Ordrer.Any())
    {
        db.Ordrer.AddRange(
            new Ordre { KundeNavn = "Anders And", Dato = DateTime.Today },
            new Ordre { KundeNavn = "Mickey Mouse", Dato = DateTime.Today }
        );
        db.SaveChanges();
    }
}
```

- `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=mysql;port=3306;database=ordredb;user=user;password=password"
  }
}
```


### Automatisk databaseinitiering og seed-data

**Formål:** Opretter tabeller og lægger demo-kunder ind ved opstart.

### 6. Tilføj database-migrationer i KundeService
Kør følgende i terminalen fra `KundeService`-mappen:
```bash
dotnet ef migrations add InitialCreate
```
> Dette genererer migrationskoden og forbereder databasen.

### 7. Tilføj seed-data og migrering i `Program.cs`
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KundeContext>();
    db.Database.Migrate();

    // Seed kun hvis tom
    if (!db.Kunder.Any())
    {
        db.Kunder.AddRange(
            new Kunde { Navn = "Anders And" },
            new Kunde { Navn = "Mickey Mouse" }
        );
        db.SaveChanges();
    }
}
```
> Dette sikrer at databasen oprettes og udfyldes første gang containeren starter.

**🗄️ Formål:** Tilføje en realistisk databaseforbindelse til fx KundeService med MySQL

### 1. Udvid `docker-compose.yml` med MySQL
```yaml
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: kundedb
      MYSQL_USER: user
      MYSQL_PASSWORD: password
    ports:
      - "3306:3306"
```

### 2. Installer EF Core og MySQL-driver i `KundeService`
```bash
cd KundeService

dotnet add package Microsoft.EntityFrameworkCore

dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### 3. Tilføj DbContext og model
`Models/Kunde.cs`
```csharp
public class Kunde
{
    public int Id { get; set; }
    public string Navn { get; set; }
}
```

`Data/KundeContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;

public class KundeContext : DbContext
{
    public KundeContext(DbContextOptions<KundeContext> options) : base(options) {}
    public DbSet<Kunde> Kunder => Set<Kunde>();
}
```

### 4. Registrér context i `Program.cs`
```csharp
builder.Services.AddDbContext<KundeContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
```

### 5. Tilføj `appsettings.json` til KundeService
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=mysql;port=3306;database=kundedb;user=user;password=password"
  }
}
```

➡️ Du kan nu bruge databasen i dine controllere og lave fx `GET`, `POST`, `PUT` m.m.

---

## Del 10: Sikkerhedsrefleksion og hardening
**🔐 Formål:** Forbinde arbejdet med microservices til OWASP Top 10, JWT-sikkerhed og reel risikovurdering

### Undervisningsflow 09:00–14:10
| Tid         | Indhold                                                                 |
|--------------|--------------------------------------------------------------------------|
| **09:00–10:00** | Teori: JWT, OWASP Top 10, sikkerhedsarkitektur                      |
| **10:00–10:30** | Demo: Udnyt et usikret API (fjern `.RequireAuthorization()`)        |
| **10:30–11:00** | Refleksion & diskussion (OWASP i forhold til jeres arkitektur)      |
| **11:00–11:45** | Opstart på microservice-øvelse – forklaring + første service         |
| **12:00–12:30** | Frokost                                                               |
| **12:30–14:10** | Øvelse fortsætter: Gateway + test + refleksion (angreb og forsvar)   |

### Øvelse: Sammenlign med Juice Shop
Besøg [https://juice-shop.herokuapp.com](https://juice-shop.herokuapp.com) og prøv disse challenges:

- **Login uden kodeord**  
  👉 Hvad hvis vores Gateway ikke validerede tokens?

- **Få adgang til en anden brugers data**  
  👉 Hvad hvis vi ikke isolerede adgang pr. bruger eller claim?

- **Post en anmeldelse uden auth**  
  👉 Hvad hvis `.RequireAuthorization()` manglede?

Diskutér: Hvordan ville man forsøge at angribe jeres egne services?

### Mini-udfordring (individuelt eller i grupper)
- Fjern sikkerheden i jeres Gateway midlertidigt
- Prøv at kalde `POST /kunde` uden token
- Prøv at kalde `POST /kunde` med forkerte data
- Prøv at sende et manipuleret token (brug fx [jwt.io](https://jwt.io))

💬 **Spørgsmål til refleksion**:
- Hvad kan vi forbedre?
- Hvilke OWASP-trusler er vi stadig udsat for?
- Hvad ville næste skridt i hardening være?

---

## Bonus
**✨ Udvidelser:**
- Swagger UI i alle services
- Brug claims og policies
- Kommunikation mellem services (fx ordre henter produkt)
- Tilføj database (fx SQLite, Postgres)
- Deploy til Kubernetes

---

Er du klar til at gå i gang?
