# Opdateret Øvelse: Microservices med Ubuntu + YARP + MySQL

## ✅ Tjekliste: Virker systemet som forventet?

| Testpunkt | Hvad du skal tjekke | Hvordan |
|-----------|----------------------|--------|
| 🟢 Gateway kører | Forsiden vises i browseren | Besøg `http://localhost:8000` |
| 🟢 Link til kunde, produkt og ordre virker | Klik på links – de går via gatewayen | Linkene skal ramme `/kunde`, `/produkt`, `/ordre` |
| 🟢 API-endpoints virker med token | API'er skal svare korrekt via gateway | Brug `curl` eller Swagger med `Authorization: Bearer demo-token` |
| 🟢 Data gemmes i databasen | POST til fx `/kunde` og tjek med GET | Brug Swagger eller curl |
| 🟢 Swagger virker | Kan du se Swagger UI i browseren? | `http://localhost:6001/swagger` osv. |

## 📁 Formål: Få overblik over mappestruktur og filplacering

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

## 🎯 Læringsmål

Ved endt øvelse vil deltageren kunne:
- Bygge og deploye .NET-baserede microservices
- Anvende YARP som API Gateway med routing og auth
- Integrere MySQL databaser i containerbaseret miljø
- Tilføje statiske HTML-filer i ASP.NET Gateway
- Teste microservices via gateway med curl og browser

## 🧠 Forudsætninger

- Grundlæggende kendskab til C# og .NET
- Kendskab til REST, HTTP og JSON
- Introduktion til Docker og Docker Compose
- Evt. kendskab til SQL og databaseforbindelser

## 🛠️ Fejlsøgning

- Tjek kørende containere:
  ```bash
  docker ps
  ```
- Se logs fra en service:
  ```bash
  docker logs ordre
  ```
- Genstart hele miljøet (inkl. volumes):
  ```bash
  docker-compose down -v
  docker-compose up --build
  ```
- Tjek databaseforbindelser: Er connection strings korrekte?
- Brug `curl` til at teste gatewayen direkte – fx med token:
  ```bash
  curl -H "Authorization: Bearer demo-token" http://localhost:8000/kunde
  ```

## Installation af Ubuntu Server og forberedelse

Før du starter, skal du bruge en virtuel maskine eller fysisk maskine med **Ubuntu Server 22.04 LTS**.

1. **Installer Ubuntu Server**
   - Download ISO fra [https://ubuntu.com/download/server](https://ubuntu.com/download/server)
   - Installer med standardindstillinger, evt. tilføj OpenSSH under installationen

2. **Installer nødvendige værktøjer**
   Log ind på Ubuntu og kør:
   ```bash
   sudo apt update
   sudo apt install docker.io docker-compose -y
   sudo usermod -aG docker $USER
   sudo reboot
   ```

Efter genstart er du klar til at opsætte projektet.

## Opsætning fra terminal (Ubuntu)

1. **Opret projektmappe:**
```bash
mkdir MicroserviceDemo
cd MicroserviceDemo
```

2. **Opret `docker-compose.yml` med nano:**
```bash
nano docker-compose.yml
```
Indsæt her YAML'en som vist senere i dokumentet.

3. **Opret undermapper og filer:**
```bash
mkdir Gateway KundeService ProduktService OrdreService
cd Gateway && nano Program.cs && cd ..
cd KundeService && nano Program.cs && cd ..
cd ProduktService && nano Program.cs && cd ..
cd OrdreService && nano Program.cs && cd ..
```

Gentag for nødvendige filer som `*.csproj`, `Dockerfile`, `Controllers/*.cs`, `Models/*.cs`, osv.

> Du kan også oprette `index.html`:
```bash
mkdir -p Gateway/wwwroot
nano Gateway/wwwroot/index.html
```

## 🗄️ Hvordan fungerer databaserne i microservices?

I dette setup med `docker-compose` har **hver microservice sin egen databasecontainer**:

| Service         | Database-container | Adgangsforbindelse                     |
|-----------------|--------------------|----------------------------------------|
| KundeService    | `mysql_kunde`      | `server=mysql_kunde;...`               |
| ProduktService  | `mysql_produkt`    | `server=mysql_produkt;...`             |
| OrdreService    | `mysql_ordre`      | `server=mysql_ordre;...`               |

### 🔐 Hvorfor adskille databaserne?
- **Isolation:** Services kan ændre schema uden at påvirke andre
- **Sikkerhed:** Ingen adgang til andres data
- **Skalering:** Du kan skalere fx ProduktService uden at påvirke resten
- **Ejerskab:** Hvert team ejer og vedligeholder sin egen datamodel

### 🤝 Hvad hvis én service har brug for en andens data?
Så skal det ske via **et API-kald**, fx:
```csharp
// Inde i OrdreService:
var kunde = await httpClient.GetFromJsonAsync<Kunde>("http://gateway/kunde/42");
```

➡️ **Del aldrig databasen direkte** – eksponér data gennem en **offentlig endpoint** i den pågældende service.

---

## Ændringer i opsætning

1. **Ubuntu som base-setup:**
   - Alle tests og opsætning forudsætter en *Ubuntu Server* VM (f.eks. Ubuntu 22.04 LTS).
   - Docker og Docker Compose skal installeres på Ubuntu-maskinen:
     ```bash
     sudo apt update
     sudo apt install docker.io docker-compose -y
     sudo usermod -aG docker $USER
     ```

2. **API Gateway med YARP:**
   - Gateway-projektet (C#) anvender YARP til at rute kald til microservices via reverse proxy.

3. **Microservices:**
   - OrdreService
   - ProduktService
   - KundeService
   
   Hver service:
   - Skrives i C# (ASP.NET Web API)
   - Kører i sin egen container
   - Bruger *egen MySQL database*

4. **HTML forside via gatewayen:**
   - `index.html` placeres i Gateway-projektet
   - Indeholder knapper/links til `/kunde`, `/produkt`, `/ordre`
   - Disse aktiverer kald gennem gatewayen til respektive microservices

5. **Docker Compose oversigt (udvidet):**
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

6. **JWT-beskyttelse i Gateway**

Program.cs (Gateway):
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

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

app.UseStaticFiles();
app.MapReverseProxy().RequireAuthorization();

app.Run("http://0.0.0.0:80");
```

7. **Swagger i alle services**

Tilføj i hver services `Program.cs`:
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

➡️ Du kan derefter tilgå Swagger på fx:
- `http://localhost:6001/swagger`
- `http://localhost:6002/swagger`
- `http://localhost:6003/swagger`

8. **Refleksionsspørgsmål og sikkerhedstest**

💬 Diskutér i grupper:
- Hvilke endpoints kræver token?
- Hvad sker der, hvis `.RequireAuthorization()` fjernes i gateway?
- Hvad sker der, hvis du sender et manipuleret token?

Test via:
```bash
# Uden token – skal fejle
curl http://localhost:8000/kunde

# Med forkert token – skal også fejle
curl -H "Authorization: Bearer bad-token" http://localhost:8000/produkt

# Med korrekt token – skal virke
curl -H "Authorization: Bearer demo-token" http://localhost:8000/ordre
```

📌 Diskutér:
- Hvordan relaterer dette til OWASP Top 10?
- Hvordan kan man forbedre sikkerheden i dette setup?

9. **KundeService kodeeksempel**

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

10. **ProduktService kodeeksempel**

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
    public async Task<IActionResult> Post([FromBody] Produkt produkt)
    {
        _context.Produkter.Add(produkt);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = produkt.Id }, produkt);
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


11. **Refleksionsøvelse: Hvad har vi bygget – og hvordan kan det forbedres?**

💬 Overvej og diskuter følgende:

### 🧩 Arkitektur
- Hvorfor bruger vi en gateway fremfor direkte adgang til services?
- Hvordan ville det fungere med flere gateways eller load balancing?

### 🔐 Sikkerhed
- Er vores token-beskyttelse god nok i en rigtig verden?
- Hvordan kunne vi bruge claims eller scopes til at adskille brugerroller?

### 🗃️ Data og skalering
- Hvad sker der, hvis én database bliver langsom?
- Skal hver service nødvendigvis have sin egen database?
- Hvordan kan vi synkronisere data mellem services?

### 💡 Udvidelser
- Tilføj en fjerde service (fx LagerService eller BrugerService)
- Lav frontend med JavaScript der kalder API'en dynamisk
- Deploy hele systemet til cloud eller Kubernetes

➡️ Afslut med en gruppefremlæggelse eller skriftlig opsamling: *“Hvad ville du gøre anderledes i en produktion?”*

---
