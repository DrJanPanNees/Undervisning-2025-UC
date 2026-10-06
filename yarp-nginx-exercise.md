# 🐳 Øvelse: YARP som Reverse Proxy foran 2 Nginx-containere

I denne øvelse lærer du at sætte en **reverse proxy med YARP** op foran to forskellige Nginx-containere.  
Formålet er at forstå hvordan en proxy kan fordele trafik til flere bagvedliggende services.

---

## 🎯 Læringsmål
- Bruge **Docker Compose** til at starte flere containere.  
- Opsætte **Nginx** som simpel statisk webserver.  
- Opsætte **YARP (Yet Another Reverse Proxy)** i .NET som en reverse proxy.  
- Rute trafik fra én indgang (port 4000) videre til to forskellige Nginx-websites.  

---
## Opstart af projektet:
```
mkdir YarpProxy && cd YarpProxy
dotnet new web
dotnet add package Yarp.ReverseProxy
```
---

## 📂 Projektstruktur

```
reverseproxy-demo/
├── docker-compose.yml
├── html1/
│   └── index.html
├── html2/
│   └── index.html
└── YarpProxy/
    ├── Program.cs
    └── appsettings.json
```

---

## 1️⃣ Docker Compose

**docker-compose.yml**
```yaml
version: "3.9"

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

---

## 2️⃣ HTML sider

**html1/index.html**
```html
<!DOCTYPE html>
<html>
<head><title>Forside</title></head>
<body>
  <h1>Velkommen til Nginx 1</h1>
  <p>Denne side kører på container nginx1 (port 5001).</p>
</body>
</html>
```

**html2/index.html**
```html
<!DOCTYPE html>
<html>
<head><title>About</title></head>
<body>
  <h1>Om denne side</h1>
  <p>Denne side kører på container nginx2 (port 5002).</p>
</body>
</html>
```

---

## 3️⃣ YARP Konfiguration

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

---

## 4️⃣ YARP C# Program

**YarpProxy/Program.cs**
```csharp
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Load YARP routes from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run("http://localhost:4000");
```

---

## 5️⃣ Kørsel

1. Start Nginx containere:
   ```bash
   docker compose up -d
   ```

2. Start YARP-projektet:
   ```bash
   dotnet run
   ```

3. Åbn i browseren:
   - [http://localhost:4000/site1/index.html](http://localhost:4000/site1/index.html) → viser Nginx1  
   - [http://localhost:4000/site2/index.html](http://localhost:4000/site2/index.html) → viser Nginx2  

---

## 6️⃣ Diagram

```
[ Browser ]
     |
     v
http://localhost:4000
     |
     +--> /site1  ----> Nginx1 (5001) ----> index.html
     |
     +--> /site2  ----> Nginx2 (5002) ----> index.html
```
