# 🧪 Microservices med Ubuntu, YARP og MySQL

## 🛍 Introduktion

Dette projekt viser, hvordan man opsætter en komplet microservice-arkitektur med:
- 3 uafhængige microservices (Kunde, Produkt, Ordre)
- En API Gateway med YARP (reverse proxy)
- MySQL databaser til hver service
- En statisk HTML-forside
- Docker Compose som orchestration

📌 **Hvorfor er det vigtigt?** Microservices giver bedre skalerbarhed, løsrivelse af komponenter og mulighed for hurtigere udvikling og vedligehold. Med denne øvelse får du praktisk erfaring i at sætte hele miljøet op.

Du kan bruge dette som øvelse, demo eller udgangspunkt for videreudvikling.

---

## 🎯 Læringsmål

Ved endt øvelse vil du kunne:
- Bygge og deploye .NET-baserede microservices
- Anvende YARP som API Gateway med routing og auth
- Integrere MySQL databaser i containerbaseret miljø
- Tilføje statiske HTML-filer i ASP.NET Gateway
- Teste microservices via gateway med curl og browser
- Forstå JWT-token og adgangskontrol i microservice-setup

---

## 🧠 Forudsætninger

- Grundlæggende kendskab til C# og .NET
- Kendskab til REST, HTTP og JSON
- Introduktion til Docker og Docker Compose
- Evt. kendskab til SQL og databaseforbindelser

---

## ⚙️ 1. Opsætning af miljø (Ubuntu)

### 📦 Installation af Docker på Ubuntu Server 22.04 LTS

```bash
sudo apt update
sudo apt install docker.io
sudo apt install docker-compose

# Find dit brugernavn og tilføj det til docker-gruppen
whoami
sudo usermod -aG docker <dit-brugernavn>

# Genstart systemet
sudo reboot
```

💡 Alternativt kan du bruge det officielle Docker-installationsscript, hvis du vil sikre dig den nyeste version:
```bash
curl -fsSL https://get.docker.com | sudo sh
```

### 📁 Projektstruktur

```bash
mkdir MicroserviceDemo
cd MicroserviceDemo
mkdir Gateway KundeService ProduktService OrdreService
```

---

## 🗂️ Fil- og mappestruktur

```bash
/MicroserviceDemo
├── docker-compose.yml
├── Gateway
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   ├── Gateway.csproj
│   └── wwwroot
│       └── index.html
├── KundeService
│   ├── Program.cs
│   ├── Kunde.cs
│   ├── KundeContext.cs
│   ├── Controllers
│   │   └── KundeController.cs
│   ├── Dockerfile
│   └── KundeService.csproj
├── ProduktService
│   ├── Program.cs
│   ├── Produkt.cs
│   ├── ProduktContext.cs
│   ├── Controllers
│   │   └── ProduktController.cs
│   ├── Dockerfile
│   └── ProduktService.csproj
└── OrdreService
    ├── Program.cs
    ├── Ordre.cs
    ├── OrdreContext.cs
    ├── Controllers
    │   └── OrdreController.cs
    ├── Dockerfile
    └── OrdreService.csproj
```

📌 **Bemærk**: Hver service har sin egen mappe og Dockerfile. Husk at ændre navnet på .dll-filen i Dockerfile til den relevante service (fx `ProduktService.dll`, `OrdreService.dll`).

---

### 📄 Gateway appsettings.json

```json
{
  "ReverseProxy": {
    "Routes": {
      "kunde": {
        "ClusterId": "kundeCluster",
        "Match": { "Path": "/kunde/{**catch-all}" }
      },
      "produkt": {
        "ClusterId": "produktCluster",
        "Match": { "Path": "/produkt/{**catch-all}" }
      },
      "ordre": {
        "ClusterId": "ordreCluster",
        "Match": { "Path": "/ordre/{**catch-all}" }
      }
    },
    "Clusters": {
      "kundeCluster": {
        "Destinations": {
          "dest1": { "Address": "http://kunde:80/" }
        }
      },
      "produktCluster": {
        "Destinations": {
          "dest1": { "Address": "http://produkt:80/" }
        }
      },
      "ordreCluster": {
        "Destinations": {
          "dest1": { "Address": "http://ordre:80/" }
        }
      }
    }
  }
}
```

---

### 🐳 Dockerfile til microservices

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KundeService.dll"]
```

🔁 **Vigtigt:** Dockerfile bruges af hver enkelt service til at starte .NET-applikationen, og du skal ændre navnet i `ENTRYPOINT` til den rigtige `.dll`-fil:

- `KundeService/Dockerfile`: `ENTRYPOINT ["dotnet", "KundeService.dll"]`
- `ProduktService/Dockerfile`: `ENTRYPOINT ["dotnet", "ProduktService.dll"]`
- `OrdreService/Dockerfile`: `ENTRYPOINT ["dotnet", "OrdreService.dll"]`

Ellers vil containeren prøve at starte den forkerte service og fejle.

---

### 🧩 docker-compose.yml

```yaml
version: '3.9'
services:
  kunde:
    build: ./KundeService
    ports: ["6001:80"]
    environment:
      - ConnectionStrings__DefaultConnection=server=mysql_kunde;port=3306;database=kundedb;user=user;password=password
    depends_on:
      - mysql_kunde

  produkt:
    build: ./ProduktService
    ports: ["6002:80"]
    environment:
      - ConnectionStrings__DefaultConnection=server=mysql_produkt;port=3306;database=produktdb;user=user;password=password
    depends_on:
      - mysql_produkt

  ordre:
    build: ./OrdreService
    ports: ["6003:80"]
    environment:
      - ConnectionStrings__DefaultConnection=server=mysql_ordre;port=3306;database=ordredb;user=user;password=password
    depends_on:
      - mysql_ordre

  gateway:
    build: ./Gateway
    ports: ["8000:80"]
    volumes:
      - ./Gateway/wwwroot:/app/wwwroot
    depends_on:
      - kunde
      - produkt
      - ordre

  mysql_kunde:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: kundedb
      MYSQL_USER: user
      MYSQL_PASSWORD: password
    ports: ["3307:3306"]

  mysql_produkt:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: produktdb
      MYSQL_USER: user
      MYSQL_PASSWORD: password
    ports: ["3308:3306"]

  mysql_ordre:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: ordredb
      MYSQL_USER: user
      MYSQL_PASSWORD: password
    ports: ["3309:3306"]
```

---

### 🌐 HTML forside

```html
<!DOCTYPE html>
<html>
<head><title>Microservice Demo</title></head>
<body>
  <h1>Velkommen til MicroserviceDemo</h1>
  <ul>
    <li><a href="/kunde">Kunde</a></li>
    <li><a href="/produkt">Produkt</a></li>
    <li><a href="/ordre">Ordre</a></li>
  </ul>
</body>
</html>
```

---

### 🌐 Gateway Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            SignatureValidator = (token, _) => new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy().RequireAuthorization();

app.Run();
```

---

### 👤 KundeService

**Kunde.cs**
```csharp
public class Kunde
{
    public int Id { get; set; }
    public string Navn { get; set; }
}
```

**KundeContext.cs**
```csharp
using Microsoft.EntityFrameworkCore;

public class KundeContext : DbContext
{
    public KundeContext(DbContextOptions<KundeContext> options) : base(options) {}
    public DbSet<Kunde> Kunder => Set<Kunde>();
}
```

**KundeController.cs**
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

**Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<KundeContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KundeContext>();
    db.Database.Migrate();

    if (!db.Kunder.Any())
    {
        db.Kunder.AddRange(
            new Kunde { Navn = "Anders And" },
            new Kunde { Navn = "Mickey Mouse" }
        );
        db.SaveChanges();
    }
}

app.Run();
```

---

### 📦 ProduktService

**Produkt.cs**
```csharp
public class Produkt
{
    public int Id { get; set; }
    public string Navn { get; set; }
    public decimal Pris { get; set; }
}
```

**ProduktContext.cs**
```csharp
using Microsoft.EntityFrameworkCore;

public class ProduktContext : DbContext
{
    public ProduktContext(DbContextOptions<ProduktContext> options) : base(options) {}
    public DbSet<Produkt> Produkter => Set<Produkt>();
}
```

**ProduktController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class ProduktController : ControllerBase
{
    private readonly ProduktContext _context;

    public ProduktController(ProduktContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var produkter = await _context.Produkter.ToListAsync();
        return Ok(produkter);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Produkt nyProdukt)
    {
        _context.Produkter.Add(nyProdukt);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = nyProdukt.Id }, nyProdukt);
    }
}
```

**Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProduktContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

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

app.Run();
```

---

### 🧾 OrdreService

**Ordre.cs**
```csharp
public class Ordre
{
    public int Id { get; set; }
    public string KundeNavn { get; set; }
    public DateTime Dato { get; set; }
}
```

**OrdreContext.cs**
```csharp
using Microsoft.EntityFrameworkCore;

public class OrdreContext : DbContext
{
    public OrdreContext(DbContextOptions<OrdreContext> options) : base(options) {}
    public DbSet<Ordre> Ordrer => Set<Ordre>();
}
```

**OrdreController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class OrdreController : ControllerBase
{
    private readonly OrdreContext _context;

    public OrdreController(OrdreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var ordrer = await _context.Ordrer.ToListAsync();
        return Ok(ordrer);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Ordre nyOrdre)
    {
        _context.Ordrer.Add(nyOrdre);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = nyOrdre.Id }, nyOrdre);
    }
}
```

**Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrdreContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

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

app.Run();
```

---, så dokumentet ikke bliver for langt ét sted. Skal jeg fortsætte med det?

---

## 🧪 4. Kørsel og test

```bash
docker-compose build
docker-compose up
```

Tjek:
- `http://localhost:8000` viser forside
- Swagger på `6001`, `6002`, `6003`
- Brug `curl` til at teste via token:
```bash
curl -H "Authorization: Bearer demo-token" http://localhost:8000/kunde
```

📌 **Hvor kommer token fra?** I denne demo bruges en hardcoded token-check i gatewayen, som godtager enhver JWT uden validering. Du kan oprette et token f.eks. via https://jwt.io, men i praksis bør dette håndteres af en Identity Provider (fx Auth0 eller Duende IdentityServer).

---
