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

### Dockerfile til begge:
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
    "Routes": [
      {
        "RouteId": "kunde",
        "ClusterId": "kunde-cluster",
        "Match": {
          "Path": "/kunde/{**catch-all}"
        }
      },
      {
        "RouteId": "produkt",
        "ClusterId": "produkt-cluster",
        "Match": {
          "Path": "/produkt/{**catch-all}"
        }
      }
    ],
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

Test med curl eller browser:
```bash
curl -H "Authorization: Bearer demo-token" http://localhost:5000/kunde
curl -H "Authorization: Bearer demo-token" http://localhost:5000/produkt
```

---

## Bonus
**💡 Udvidelse:**
- Tilføj Swagger UI til services
- Udskift static HTML med backend-API'er
- Deploy i Kubernetes som øvelse

---

## 🛠️ Fejlsøgning og Førstehjælp

Her er nogle typiske fejl og hvad du kan gøre ved dem:

### 🧩 Gateway starter ikke
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
- Tjek at portene `5000`, `6001` og `6002` ikke bruges af andre programmer.
- Skift dem i `docker-compose.yml`, hvis nødvendigt.

### 🐋 Ændringer slår ikke igennem
- Kør `docker compose down` efterfulgt af `docker compose up --build` for at rydde cache og genstarte alt korrekt.

---

Held og lykke – og spørg endelig din underviser, hvis du sidder fast!

