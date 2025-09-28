# Øvelse: Microservices med YARP, Docker og MySQL

## Metadata
- **Fag**: Teknologi / Systemudvikling  
- **Tema**: Microservices, API Gateway (YARP), Docker Compose, MySQL  
- **Forudsætninger**:  
  - I har arbejdet med .NET 8 minimal APIs  
  - I kender YARP (Reverse Proxy) fra tidligere øvelser  
  - I har arbejdet med Docker og Docker Compose  
- **Hvorfor er øvelsen vigtig?**  
  Microservices er en central del af moderne softwarearkitektur.  
  I denne øvelse lærer I at:  
  1. Dele en applikation op i mindre services  
  2. Lade YARP fungere som **API Gateway**  
  3. Koble services til en fælles database i Docker  
  4. Forstå forskelle i konfiguration mellem Gateway og services  

---

## Øvelsens mål
- Opsætte **to microservices** (`ProductsService` og `CustomersService`)  
- Opsætte en **Proxy service** (YARP Gateway)  
- Koble alle services til en **MySQL database i Docker**  
- Bygge en **startside i HTML**, hvorfra man kan navigere til de enkelte services  

---

## Filstruktur
Sådan skal jeres projektmappe se ud:

```
MicroservicesDemo/
├── Proxy/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── wwwroot/
│       └── index.html
├── ProductsService/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── CustomersService/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── init/
│   ├── products-init.sql
│   └── customers-init.sql
└── docker-compose.yml
```

---

## Trin 1 – Database i Docker
Vi bruger en **fælles MySQL-database**.

👉 Opgave: Lav en mappe `init/` og tilføj to `.sql`-filer:  
- `products-init.sql` – skal oprette en `Products`-tabel og indsætte nogle testdata.  
- `customers-init.sql` – skal oprette en `Customers`-tabel og indsætte testdata.  

**Hint:** Brug `CREATE TABLE` og `INSERT INTO`.  
I kan lade jer inspirere af tidligere øvelser med SQL.

---

## Trin 2 – Docker Compose
Her får I en **skabelon** til `docker-compose.yml`:

```yaml
services:
  proxy:
    build:
      context: ./Proxy
    ports:
      - "4000:80"
    depends_on:
      - products-service
      - customers-service

  products-service:
    build:
      context: ./ProductsService
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=server=mysql-db;port=3306;database=demo;user=appuser;password=apppassword
    depends_on:
      - mysql-db

  customers-service:
    build:
      context: ./CustomersService
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=server=mysql-db;port=3306;database=demo;user=appuser;password=apppassword
    depends_on:
      - mysql-db

  mysql-db:
    image: mysql:8.0
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: demo
      MYSQL_USER: appuser
      MYSQL_PASSWORD: apppassword
    volumes:
      - mysql_data:/var/lib/mysql
      - ./init:/docker-entrypoint-initdb.d

volumes:
  mysql_data:
```

👉 Opgave:  
Forklar i jeres gruppe, hvorfor `Proxy` **ikke** har en connection string til databasen, men de andre services har.  
(Skriv et par linjer i jeres kodekommentarer).

---

## Trin 3 – Dockerfiles
Her er en fælles Dockerfile-skabelon, som kan bruges i **alle tre services**.  

```dockerfile
# Byggestage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "YourServiceName.dll"]
```

👉 Opgave:  
- I hver service skal `YourServiceName.dll` ændres til den rigtige fil (`Proxy.dll`, `ProductsService.dll`, `CustomersService.dll`).  

---

## Trin 4 – Proxy (YARP)
`Proxy/appsettings.json` (læg mærke til forskellen fra services):

```json
{
  "ReverseProxy": {
    "Routes": {
      "productsRoute": {
        "ClusterId": "productsCluster",
        "Match": { "Path": "/api/products/{**catch-all}" }
      },
      "customersRoute": {
        "ClusterId": "customersCluster",
        "Match": { "Path": "/api/customers/{**catch-all}" }
      }
    },
    "Clusters": {
      "productsCluster": {
        "Destinations": {
          "productsDestination": { "Address": "http://products-service:8080" }
        }
      },
      "customersCluster": {
        "Destinations": {
          "customersDestination": { "Address": "http://customers-service:8080" }
        }
      }
    }
  }
}
```

👉 Opgave:  
- Hvorfor peger vi her på `products-service:8080` i stedet for `localhost:5001`?  

---

## Trin 5 – Startsiden
Opret filen `Proxy/wwwroot/index.html`:

```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>Microservices Demo</title>
</head>
<body>
  <h1>Velkommen til Microservices Demo</h1>
  <ul>
    <li><a href="/api/products">Produkter</a></li>
    <li><a href="/api/customers">Kunder</a></li>
  </ul>
</body>
</html>
```

---

## Trin 6 – Kør systemet
Kør følgende kommandoer:

```bash
docker compose down -v --rmi all --remove-orphans
docker compose up --build
```

Når alt kører:  
- Startside: [http://localhost:4000](http://localhost:4000)  
- Produkter: [http://localhost:4000/api/products](http://localhost:4000/api/products)  
- Kunder: [http://localhost:4000/api/customers](http://localhost:4000/api/customers)  

---

## Aflevering
- Kommenter i koden hvor I har set forskelle mellem Proxy og Services.  
- Upload jeres projekt til GitHub.  
- Svar på opgaven: Hvorfor er det smart at have en Proxy/Gateway foran jeres microservices?
