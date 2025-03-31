# Øvelse: Byg en API Gateway med YARP og NGINX-services

I denne øvelse skal du opbygge et API Gateway-miljø med [YARP](https://github.com/microsoft/reverse-proxy) i .NET, som videresender trafik til to mikrotjenester ("Kunde" og "Produkt"), der er hostet via NGINX. Hele setup'et kører i Docker med Docker Compose.

## Mål
- Lære at konfigurere YARP til routing
- Forstå hvordan services kan ligge bag en gateway
- Træne Docker + Docker Compose workflows

---

## Del 1: Projektstruktur
**📁 Formål:** Få overblik over fil- og mappeopbygning i projektet

```
/YarpDockerDemo
├── Gateway
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── KundeService
│   ├── index.html
│   ├── default.conf
│   └── Dockerfile
├── ProduktService
│   ├── index.html
│   ├── default.conf
│   └── Dockerfile
└── docker-compose.yml
```

---

## Del 2: Opret Kunde- og Produktservices (NGINX)
**🌐 Formål:** Skabe to enkle webservices med NGINX som simulerer mikrotjenester

### KundeService

#### `KundeService/index.html`
**🌐 Formål:** Skabe to enkle webservices med NGINX som simulerer mikrotjenester

### `KundeService/index.html`
```html
<!DOCTYPE html>
<html><body><h1>KundeService</h1></body></html>
```

### `KundeService/default.conf`
```nginx
server {
    listen 80;
    location / {
        root /usr/share/nginx/html;
        index index.html;
    }
}
```

> Gentag for `ProduktService`, men ændr indholdet til f.eks. `<h1>ProduktService</h1>`

#### `KundeService/Dockerfile`
```Dockerfile
FROM nginx:alpine
COPY index.html /usr/share/nginx/html/
COPY default.conf /etc/nginx/conf.d/default.conf
```

#### `ProduktService/index.html`
```html
<!DOCTYPE html>
<html><body><h1>ProduktService</h1></body></html>
```

#### `ProduktService/default.conf`
```nginx
server {
    listen 80;
    location / {
        root /usr/share/nginx/html;
        index index.html;
    }
}
```

#### `ProduktService/Dockerfile`
```Dockerfile
FROM nginx:alpine
COPY index.html /usr/share/nginx/html/
COPY default.conf /etc/nginx/conf.d/default.conf
```Dockerfile
FROM nginx:alpine
COPY index.html /usr/share/nginx/html/
COPY default.conf /etc/nginx/conf.d/default.conf
```

---

## Del 3: Opret Gateway med YARP
**🔁 Formål:** Byg en API Gateway som proxy mellem klient og tjenester, og tilføj simpel token-baseret sikkerhed

### `Gateway/Program.cs`
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Tilføj JWT-baseret autentificering med et simpelt token-tjek
builder.Services.AddAuthentication("MyScheme")
    .AddJwtBearer("MyScheme", options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Læs Authorization-headeren og valider token manuelt
                var token = context.Request.Headers["Authorization"];
                if (token == "Bearer demo-token")
                {
                    // Hvis token er korrekt, oprettes en simpel bruger-identitet
                    context.Principal = new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity("MyScheme")
                    );
                    context.Success();
                }
                return Task.CompletedTask;
            }
        };
    });

// Tilføj autorisationstjeneste
builder.Services.AddAuthorization();

// Indlæs YARP-konfigurationen fra appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Brug autentificering og autorisation før routing
app.UseAuthentication();
app.UseAuthorization();

// Brug reverse proxy som default-route og kræv autorisation
app.MapReverseProxy().RequireAuthorization();

// Start applikationen og lyt på port 80
app.Run("http://0.0.0.0:80");
```

### `Gateway/appsettings.json`
```json
{
  "ReverseProxy": {
    "Routes": {
      "kunde": {
        "ClusterId": "kunde-cluster",
        "Match": {
          "Path": "/kunde/{**catch-all}"
        }
      },
      "produkt": {
        "ClusterId": "produkt-cluster",
        "Match": {
          "Path": "/produkt/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "kunde-cluster": {
        "Destinations": {
          "nginx1": { "Address": "http://kunde:80/" }
        }
      },
      "produkt-cluster": {
        "Destinations": {
          "nginx2": { "Address": "http://produkt:80/" }
        }
      }
    }
  }
}
```

### `Gateway/Dockerfile`
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

---

## Del 4: Docker Compose
**📦 Formål:** Samle alle services i én konfiguration og orkestrér dem med Docker Compose

### `docker-compose.yml`
```yaml
version: '3.9'
services:
  kunde:
    build: ./KundeService
    container_name: kunde
    ports:
      - "6001:80"

  produkt:
    build: ./ProduktService
    container_name: produkt
    ports:
      - "6002:80"

  gateway:
    build: ./Gateway
    container_name: gateway
    ports:
      - "5000:80"
    depends_on:
      - kunde
      - produkt
```

---

## Del 5: Test systemet
**✅ Formål:** Bekræft at gatewayen videresender trafik korrekt og kræver token

Kør det hele:
```bash
docker compose up --build
```

Hvis du bruger en alternativ port (fx 8000 i stedet for 5000), så husk at justere i testen nedenfor.

Test med curl eller browser:
```bash
curl -H "Authorization: Bearer demo-token" http://localhost:8000/kunde
curl -H "Authorization: Bearer demo-token" http://localhost:8000/produkt
```bash
curl -H "Authorization: Bearer demo-token" http://localhost:5000/kunde
curl -H "Authorization: Bearer demo-token" http://localhost:5000/produkt
```

---

## Del 6: Load Balancing & Security Policies
**🛡️ Formål:** Undersøg hvordan YARP understøtter forskellige load balancing-strategier og sikkerhedspolitikker

### Load balancing via konfiguration
Tilføj flere destinations for at simulere load balancing:
```json
"Clusters": {
  "produkt-cluster": {
    "LoadBalancingPolicy": "RoundRobin",
    "Destinations": {
      "nginx1": { "Address": "http://produkt:80/" },
      "nginx2": { "Address": "http://produkt:80/" }
    }
  }
}
```
> Test med mange gentagne requests og observer hvordan trafikken fordeles.

### Sikkerhedspolitikker via autorisation
Tilføj en simpel autorisationspolitik:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("KunKundeAdgang", policy =>
        policy.RequireClaim("role", "kunde"));
});
```
Og påfør den på proxyen:
```csharp
app.MapReverseProxy().RequireAuthorization("KunKundeAdgang");
```
> Dette kræver at du selv håndterer claims i JWT-token – uden for denne demo.

---

## Bonus
**💡 Udvidelse:**
- Tilføj Swagger UI til services
- Udskift static HTML med backend-API'er
- Deploy i Kubernetes som øvelse

---

## 🛠️ Fejlsøgning og Førstehjælp

### 🧾 Copy-paste venlig version af `appsettings.json`
Hvis gatewayen fejler med en JSON-parsing-fejl, er det ofte fordi `appsettings.json` ikke er valid JSON eller bruger et gammelt YARP-format. Brug nedenstående version:

```json
{
  "ReverseProxy": {
    "Routes": {
      "kunde": {
        "ClusterId": "kunde-cluster",
        "Match": {
          "Path": "/kunde/{**catch-all}"
        }
      },
      "produkt": {
        "ClusterId": "produkt-cluster",
        "Match": {
          "Path": "/produkt/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "kunde-cluster": {
        "Destinations": {
          "nginx1": {
            "Address": "http://kunde:80/"
          }
        }
      },
      "produkt-cluster": {
        "Destinations": {
          "nginx2": {
            "Address": "http://produkt:80/"
          }
        }
      }
    }
  }
}
```


### 📦 Manglende eller ufuldstændige NuGet-pakker i Gateway-projektet
- Hvis du ser fejl som:
  - `'JwtBearer' does not exist in the namespace 'Microsoft.AspNetCore.Authentication'`
  - `'Yarp' could not be found`
- Så mangler du nødvendige NuGet-pakker. Kør disse i `Gateway/`-mappen:
  ```bash
  dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
  dotnet add package Yarp.ReverseProxy --version 2.3.0
  ```
- Herefter bør din `Gateway.csproj` indeholde:
  ```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Yarp.ReverseProxy" Version="2.3.0" />
  </ItemGroup>
  ```
- Tip: Hvis problemet opstår under Docker-build, så ryd cachen og rebuild:
  ```bash
  docker compose build --no-cache
  docker compose up
  ```
- Hvis du ser fejl som:
  - `'JwtBearer' does not exist in the namespace 'Microsoft.AspNetCore.Authentication'`
  - `'Yarp' could not be found`
- Så mangler du nødvendige NuGet-pakker. Kør disse i `Gateway/`-mappen:
  ```bash
  dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
  dotnet add package Yarp.ReverseProxy
  ```
- Herefter kan du bygge igen med:
  ```bash
  docker compose up --build
  ```

Her er nogle typiske fejl og hvad du kan gøre ved dem:

### 🧩 Gateway starter ikke
- **Fejl: NETSDK1045 - .NET 9.0 understøttes ikke i Docker-bygning**
  - Hvis du har oprettet `Gateway.csproj` med `dotnet new`, bruger den måske `<TargetFramework>net9.0</TargetFramework>`.
  - Ret det til:
    ```xml
    <TargetFramework>net8.0</TargetFramework>
    ```
  - Gem og kør derefter:
    ```bash
    docker compose up --build
    ```
- **Sikker metode til at oprette Gateway.csproj uden at miste eksisterende filer:**
  1. Gå ind i `Gateway`-mappen
     ```bash
     cd Gateway
     ```
  2. Omdøb dine filer midlertidigt, så de ikke overskrives:
     ```bash
     mv Program.cs Program.custom.cs
     mv appsettings.json appsettings.custom.json
     ```
  3. Opret projektet:
     ```bash
     dotnet new web
     ```
  4. Slet de nye `Program.cs` og `appsettings.json`:
     ```bash
     rm Program.cs appsettings.json
     ```
  5. Gendan dine egne filer:
     ```bash
     mv Program.custom.cs Program.cs
     mv appsettings.custom.json appsettings.json
     ```

- **Fejl: `MSBUILD : error MSB1003: Specify a project or solution file.`**
- **Fejl: `MSBUILD : error MSB1003: Specify a project or solution file.`**
  - Dette betyder, at `Gateway.csproj` mangler.
  - Gå ind i `Gateway/` mappen og kør:
    ```bash
    dotnet new web
    ```
  - Dette opretter en `.csproj`-fil så `dotnet publish` virker i Docker.
  - Alternativt: Ret Dockerfile og peg eksplicit på projektfilen:
    ```dockerfile
    RUN dotnet publish Gateway.csproj -c Release -o /app/publish
    ```

- **Tjek at du har en `Gateway.csproj`** i `Gateway/` mappen.
  - Hvis ikke: kør `dotnet new web -n Gateway` for at oprette én.
- Sørg for at du har Docker og .NET SDK installeret korrekt.

### 🔐 Fejl 401 Unauthorized
- Husk at sende `Authorization` header med din request:
  ```bash
  curl -H "Authorization: Bearer demo-token" http://localhost:5000/kunde
  ```
- Token skal være nøjagtigt `demo-token` (case-sensitive).

### 🌐 Gateway kan ikke nå services
- Tjek at service-navne i `appsettings.json` matcher dem i `docker-compose.yml` (`kunde`, `produkt`).
- Prøv at pinge containerne fra gateway-containere med fx `docker exec -it gateway ping kunde`

### ⚓ Port allerede i brug
- Fejl: `bind: address already in use`
  - Det betyder at noget andet (et andet program eller en tidligere instans) bruger porten, fx 5000.
  - Find hvad der bruger porten:
    ```bash
    lsof -i :5000
    ```
  - Luk programmet med fx:
    ```bash
    kill -9 <PID>
    ```
  - Alternativt: Skift port i `docker-compose.yml`, fx fra:
    ```yaml
    ports:
      - "5000:80"
    ```
    til:
    ```yaml
    ports:
      - "5050:80"
    ```
  - Husk at teste med den nye port:
    ```bash
    curl -H "Authorization: Bearer demo-token" http://localhost:5050/kunde
    ```
- Tjek at portene `5000`, `6001` og `6002` ikke bruges af andre programmer.
- Skift dem i `docker-compose.yml`, hvis nødvendigt.

### 🐋 Ændringer slår ikke igennem
- Kør `docker compose down` efterfulgt af `docker compose up --build` for at rydde cache og genstarte alt korrekt.

---

Held og lykke – og spørg endelig din underviser, hvis du sidder fast!

