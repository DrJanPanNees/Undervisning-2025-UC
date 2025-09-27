# Øvelse: YARP med to Node.js services (Products + Weather)

## 🎯 Formål
Formålet med denne øvelse er at lære, hvordan en **C# YARP reverse proxy** kan bruges til at route trafik til flere backend-services.  
Vi bygger to simple Node.js services:  

- **Products Service** (returnerer en liste af produkter)  
- **Weather Service** (returnerer en liste af vejrobservationer)  

Og så sætter vi en **YARP proxy** foran dem, så de begge kan tilgås via én fælles indgang.

---

## 📦 Projektstruktur

```
YarpNodeDemo/
├── Proxy/              # C# YARP projekt
│   ├── Program.cs
│   ├── appsettings.json
│   └── Proxy.csproj
├── ProductsService/    # Node.js products service
│   ├── server.js
│   ├── package.json
│   └── Dockerfile
├── WeatherService/     # Node.js weather service
│   ├── server.js
│   ├── package.json
│   └── Dockerfile
└── docker-compose.yml
```

---

## 1️⃣ Products Service

**server.js**
```js
const express = require("express");
const app = express();
const PORT = 3001;

app.get("/api/products", (req, res) => {
  res.json([
    { id: 1, name: "Laptop", price: 7999 },
    { id: 2, name: "Phone", price: 4999 }
  ]);
});

app.listen(PORT, () => {
  console.log(`Products service running on http://localhost:${PORT}`);
});
```

**package.json**
```json
{
  "name": "products-service",
  "version": "1.0.0",
  "main": "server.js",
  "dependencies": {
    "express": "^4.18.2"
  }
}
```

**Dockerfile**
```dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 3001
CMD ["node", "server.js"]
```

---

## 2️⃣ Weather Service

**server.js**
```js
const express = require("express");
const app = express();
const PORT = 3002;

app.get("/api/weather", (req, res) => {
  res.json([
    { city: "Vejle", temperature: 16 },
    { city: "Aarhus", temperature: 14 }
  ]);
});

app.listen(PORT, () => {
  console.log(`Weather service running on http://localhost:${PORT}`);
});
```

**package.json**
```json
{
  "name": "weather-service",
  "version": "1.0.0",
  "main": "server.js",
  "dependencies": {
    "express": "^4.18.2"
  }
}
```

**Dockerfile**
```dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 3002
CMD ["node", "server.js"]
```

---

## 3️⃣ YARP Proxy (C#)

Kør i terminalen for at oprette projektet:

```bash
dotnet new web -n Proxy
```

**Program.cs**
```csharp
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run("http://0.0.0.0:4000");
```

**appsettings.json**
```json
{
  "ReverseProxy": {
    "Routes": {
      "productsRoute": {
        "ClusterId": "productsCluster",
        "Match": { "Path": "/api/products/{**catch-all}" }
      },
      "weatherRoute": {
        "ClusterId": "weatherCluster",
        "Match": { "Path": "/api/weather/{**catch-all}" }
      }
    },
    "Clusters": {
      "productsCluster": {
        "Destinations": {
          "products": { "Address": "http://products-service:3001" }
        }
      },
      "weatherCluster": {
        "Destinations": {
          "weather": { "Address": "http://weather-service:3002" }
        }
      }
    }
  }
}
```

**Proxy.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Yarp.ReverseProxy" Version="2.3.0" />
  </ItemGroup>

</Project>
```

---

## 4️⃣ Docker Compose

**docker-compose.yml**
```yaml
version: "3.9"

services:
  products-service:
    build: ./ProductsService
    container_name: products-service

  weather-service:
    build: ./WeatherService
    container_name: weather-service

  proxy:
    build: ./Proxy
    container_name: yarp-proxy
    ports:
      - "4000:4000"
    depends_on:
      - products-service
      - weather-service
```

---

## 🚀 Kør projektet

```bash
docker-compose up --build
```

---

## ✅ Test

- Products API: [http://localhost:4000/api/products](http://localhost:4000/api/products)  
- Weather API: [http://localhost:4000/api/weather](http://localhost:4000/api/weather)

---
