# 🐳 Øvelse: YARP som Reverse Proxy foran 2 Nginx-containere

I denne øvelse lærer du at sætte en **reverse proxy med YARP** op foran to forskellige Nginx-containere.
Formålet er at forstå, hvordan en proxy kan fordele trafik til flere bagvedliggende services.

---

## 🎯 Læringsmål

- Bruge **Docker Compose** til at starte flere containere.
- Opsætte **Nginx** som simpel statisk webserver.
- Opsætte **YARP (Yet Another Reverse Proxy)** i .NET som en reverse proxy.
- Rute trafik fra én indgang (port 4000) videre til to forskellige Nginx-websites.

## ✅ Forudsætninger

- Docker Desktop (eller Docker Engine med Compose v2) er installeret.
- .NET SDK 8.0 eller nyere er installeret (`dotnet --version`).

---

## 📂 Projektstruktur

Når du er færdig, skal din mappe se sådan ud:

```
reverseproxy-demo/
├── docker-compose.yml
├── html1/
│   └── index.html
├── html2/
│   └── index.html
└── YarpProxy/
    ├── YarpProxy.csproj
    ├── Program.cs
    └── appsettings.json
```

---

## 0️⃣ Opstart af projektet

Opret først rodmappen og de to HTML-mapper, og opret derefter YARP-projektet **inde i** rodmappen:

```bash
mkdir reverseproxy-demo && cd reverseproxy-demo
mkdir html1 html2

mkdir YarpProxy && cd YarpProxy
dotnet new web
dotnet add package Yarp.ReverseProxy
cd ..
```

> 💡 `dotnet new web` genererer selv en `Program.cs` og en `appsettings.json`.
> Du skal **erstatte indholdet** af begge filer med koden herunder.

---

## 1️⃣ Docker Compose

Opret filen i rodmappen (`reverseproxy-demo/`).

**docker-compose.yml**
```yaml
services:
  nginx1:
    image: nginx:alpine
    container_name: nginx1
    ports:
      - "5001:80"
    volumes:
      - ./html1:/usr/share/nginx/html:ro

  nginx2:
    image: nginx:alpine
    container_name: nginx2
    ports:
      - "5002:80"
    volumes:
      - ./html2:/usr/share/nginx/html:ro
```

> 💡 `version:` er udgået i Docker Compose v2 og giver en advarsel, så den er fjernet.

---

## 2️⃣ HTML-sider

**html1/index.html**
```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>Forside</title>
</head>
<body>
  <h1>Velkommen til Nginx 1</h1>
  <p>Denne side kører på container nginx1 (port 5001).</p>
</body>
</html>
```

**html2/index.html**
```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>About</title>
</head>
<body>
  <h1>Om denne side</h1>
  <p>Denne side kører på container nginx2 (port 5002).</p>
</body>
</html>
```

> 💡 `<meta charset="UTF-8">` er tilføjet, så danske tegn (æ, ø, å) vises korrekt.

---

## 3️⃣ YARP-konfiguration

**YarpProxy/appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ReverseProxy": {
    "Routes": {
      "nginx1Route": {
        "ClusterId": "nginx1Cluster",
        "Match": {
          "Path": "/site1/{**catch-all}"
        },
        "Transforms": [
          { "PathRemovePrefix": "/site1" }
        ]
      },
      "nginx2Route": {
        "ClusterId": "nginx2Cluster",
        "Match": {
          "Path": "/site2/{**catch-all}"
        },
        "Transforms": [
          { "PathRemovePrefix": "/site2" }
        ]
      }
    },
    "Clusters": {
      "nginx1Cluster": {
        "Destinations": {
          "nginx1Backend": {
            "Address": "http://localhost:5001/"
          }
        }
      },
      "nginx2Cluster": {
        "Destinations": {
          "nginx2Backend": {
            "Address": "http://localhost:5002/"
          }
        }
      }
    }
  }
}
```

**Sådan virker det:**

- `Routes` bestemmer, **hvilke URL'er** proxyen reagerer på.
- `PathRemovePrefix` fjerner `/site1` (eller `/site2`), før requesten sendes videre. Nginx kender ikke til `/site1`, så uden denne transform ville du få 404.
- `Clusters` bestemmer, **hvortil** trafikken sendes. Her peger de på de porte, Docker Compose har eksponeret på din maskine.

---

## 4️⃣ YARP C# Program

**YarpProxy/Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Hent YARP-routes og -clusters fra appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// /site1 og /site2 uden afsluttende "/" matcher ikke YARP-routerne,
// så vi sender dem videre til versionen med "/".
// Det er vigtigt at sammenligne den præcise sti (MapGet("/site1") ville
// også ramme "/site1/" og skabe et redirect-loop).
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path == "/site1" || path == "/site2")
    {
        context.Response.Redirect(path + "/");
        return;
    }
    await next();
});

app.MapReverseProxy();

app.Run("http://localhost:4000");
```

> 💡 `using Yarp.ReverseProxy;` er fjernet, da det ikke er nødvendigt. `AddReverseProxy()` ligger i
> `Microsoft.Extensions.DependencyInjection`, som allerede er importeret automatisk i .NET 6+.

---

## 5️⃣ Kørsel

1. Start Nginx-containerne fra **rodmappen** (`reverseproxy-demo/`):
   ```bash
   docker compose up -d
   ```
   Kontrollér, at begge kører:
   ```bash
   docker compose ps
   ```

2. Test Nginx direkte, **før** du involverer YARP:
   - <http://localhost:5001> → skal vise Nginx 1
   - <http://localhost:5002> → skal vise Nginx 2

3. Start YARP-projektet fra **YarpProxy-mappen**:
   ```bash
   cd YarpProxy
   dotnet run
   ```

4. Åbn i browseren:
   - <http://localhost:4000/site1/> → viser Nginx 1
   - <http://localhost:4000/site2/> → viser Nginx 2
   - `/site1/index.html` og `/site2/index.html` virker også.

5. Luk ned igen, når du er færdig:
   - Stop YARP med `Ctrl + C`.
   - Stop containerne med `docker compose down` (fra rodmappen).

---

## 6️⃣ Diagram

```
[ Browser ]
     |
     v
http://localhost:4000   (YARP)
     |
     +--> /site1/...  ----> http://localhost:5001  (Nginx1) ----> html1/index.html
     |
     +--> /site2/...  ----> http://localhost:5002  (Nginx2) ----> html2/index.html
```

---

## 🛠️ Fejlfinding

| Problem | Mulig årsag og løsning |
|---|---|
| `404` på `http://localhost:4000/` | Forventet. Der er kun routes for `/site1` og `/site2`. |
| `ERR_TOO_MANY_REDIRECTS` | Redirect-koden i `Program.cs` rammer også stien med `/`. Brug middleware-versionen ovenfor, der sammenligner den præcise sti. |
| `404` på `/site1/index.html` | Tjek at `html1/index.html` findes, og at `PathRemovePrefix` er sat. |
| `502 Bad Gateway` | Nginx-containerne kører ikke. Kør `docker compose ps` og `docker compose up -d`. |
| `port is already allocated` | Port 5001/5002 er optaget. Skift porten i både `docker-compose.yml` og `appsettings.json`. |
| `Address already in use` ved 4000 | Et andet program bruger port 4000. Luk det, eller skift porten i `Program.cs`. |
| Ændringer i `appsettings.json` virker ikke | Stop og start `dotnet run` igen. |

---

## 🚀 Ekstra udfordringer

1. Tilføj en **tredje** Nginx-container (`nginx3`, port 5003) og en tilhørende route `/site3`.
2. Giv **ét cluster to destinations** (fx to containere med samme indhold) og se, hvordan YARP load balancer. Brug `"LoadBalancingPolicy": "RoundRobin"` på clusteret.
3. Læg YARP selv i en container, og erstat `localhost:5001` med `http://nginx1:80` (Docker-netværkets servicenavn). Overvej, hvorfor `localhost` ikke virker inde fra en container.
4. Hvad sker der, hvis siderne indeholder links som `/style.css`? Hvorfor går de i stykker bag et prefix som `/site1`?
