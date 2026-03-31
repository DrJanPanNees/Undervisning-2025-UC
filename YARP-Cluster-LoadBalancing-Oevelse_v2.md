# 🐳 Øvelse: YARP med Cluster og Load Balancing

I denne øvelse skal du opsætte **YARP som API Gateway**, hvor **ét cluster indeholder flere backend‑instanser**, så YARP kan fordele trafikken (load balancing).

Øvelsen følger samme struktur og niveau som de eksisterende YARP‑øvelser i undervisningsmaterialet.

---

## 🎯 Læringsmål

Efter øvelsen kan du:

- Forklare forskellen på routing og load balancing
- Opsætte et YARP **Cluster med flere destinations**
- Forstå hvordan ét cluster svarer til én **skaleret microservice**
- Forklare YARPs rolle i horisontal skalering

---

## 📂 Projektstruktur

```Shell
yarp-loadbalancing-demo/
├── docker-compose.yml
├── backend/
│   ├── server.js
│   ├── package.json
│   └── Dockerfile
└── gateway/
    ├── Dockerfile
    ├── Program.cs
    ├── appsettings.json
    └── Gateway.csproj
```

---

## 1️⃣ Backend service (simuleret microservice)

Backend‑servicen returnerer hvilken instans der håndterer requesten. Det gør load balancing synlig.

### backend/server.js

```Javascript
const express = require("express");
const app = express();

const PORT = 3000;
const INSTANCE = process.env.INSTANCE_NAME || "unknown";

app.get("/api/booking", (req, res) => {
  res.json({
    service: "BookingService",
    instance: INSTANCE,
    timestamp: new Date().toISOString()
  });
});

app.listen(PORT, () => {
  console.log(`BookingService (${INSTANCE}) running on port ${PORT}`);
});
```

### backend/package.json

```Json
{
  "name": "booking-service",
  "version": "1.0.0",
  "main": "server.js",
  "dependencies": {
    "express": "^4.18.2"
  }
}
```

### backend/Dockerfile

```Dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 3000
CMD ["node", "server.js"]
```

---

## 2️⃣ YARP Gateway

### gateway/Program.cs

```CSharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run("http://0.0.0.0:4000");
```

### gateway/Gateway.csproj

```Xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Yarp.ReverseProxy" Version="2.3.0" />
  </ItemGroup>
</Project>
```

### gateway/Dockerfile

```Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 4000
ENTRYPOINT ["dotnet", "Gateway.dll"]
```

---

## 3️⃣ YARP konfiguration – Cluster med Load Balancing

### gateway/appsettings.json

```Json
{
  "ReverseProxy": {
    "Routes": {
      "bookingRoute": {
        "ClusterId": "bookingCluster",
        "Match": {
          "Path": "/api/booking/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "bookingCluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "booking1": {
            "Address": "http://booking1:3000"
          },
          "booking2": {
            "Address": "http://booking2:3000"
          },
          "booking3": {
            "Address": "http://booking3:3000"
          }
        }
      }
    }
  }
}
```

---

## 4️⃣ Docker Compose – flere instanser

### docker-compose.yml

```Yaml
services:
  booking1:
    build: ./backend
    environment:
      INSTANCE_NAME: "booking-1"

  booking2:
    build: ./backend
    environment:
      INSTANCE_NAME: "booking-2"

  booking3:
    build: ./backend
    environment:
      INSTANCE_NAME: "booking-3"

  gateway:
    build: ./gateway
    ports:
      - "4000:4000"
    depends_on:
      - booking1
      - booking2
      - booking3
```

---

## 5️⃣ Kør systemet

```Shell
docker compose up --build
```

---

## 6️⃣ Test load balancing

```Shell
http://localhost:4000/api/booking
```
Prøv at refresh og se at den ændre adressen.
---

## 🧠 Refleksion

- Hvad repræsenterer clusteret i denne øvelse?
- Hvad er forskellen på routing og load balancing?
- Hvor mange ændringer kræver det at tilføje endnu en instans?
- Hvorfor behøver klienten ikke vide, hvilken instans der svarer?

---

## ✅ Opsummering

- **Route** bestemmer hvornår en request matches
- **Cluster** bestemmer hvor trafikken sendes hen
- **Flere destinations i ét cluster = load balancing**
- Ét cluster svarer til én **skaleret microservice**
