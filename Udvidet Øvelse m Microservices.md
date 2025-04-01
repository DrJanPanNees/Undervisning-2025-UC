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
**👤 Formål:** Skabe en enkel C#-baseret API for kundeoplysninger

### `KundeController.cs`
**👤 Formål:** Skabe en enkel C#-baseret API for kundeoplysninger

- Lav en Web API med:
  ```bash
  dotnet new webapi -n KundeService
  ```

- Erstat `WeatherForecastController` med denne:
```csharp
[ApiController]
[Route("[controller]")]
public class KundeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { navn = "Test Kunde" });
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
**🧭 Formål:** Brug YARP til at rute trafik til de tre services

- Opret `Gateway` med `dotnet new web -n Gateway`
- Tilføj token-validering i `Program.cs` (samme som tidligere øvelse)
- Definér ruter og destinationer i `appsettings.json`:
```json
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
**🧪 Formål:** Verificér at alt virker via gatewayen

Test med:
```bash
curl -H "Authorization: Bearer demo-token" http://localhost:8000/kunde
curl -H "Authorization: Bearer demo-token" http://localhost:8000/produkt
curl -H "Authorization: Bearer demo-token" http://localhost:8000/ordre
```

---

## Del 8: Tilføj Swagger UI til alle services
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

## Bonus
**✨ Udvidelser:**
- Swagger UI i alle services
- Brug claims og policies
- Kommunikation mellem services (fx ordre henter produkt)
- Tilføj database (fx SQLite, Postgres)
- Deploy til Kubernetes

---

Er du klar til at gå i gang?
