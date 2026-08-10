# Guidet øvelse: Fra lokal C# WebApp til Docker (docker compose)

## Metadata (til underviser & ITSlearning)

**Uddannelse:** Datamatiker  
**Semester:** 2.–3. semester (kan justeres)  
**Varighed:** 1 undervisningsdag (ca. 4–6 timer inkl. pauser)  
**Arbejdsform:** Guidet øvelse + refleksion i grupper  
**Forudsætninger:** Grundlæggende C#, basal terminalbrug  
**Teknologier:** .NET ??, ASP.NET Core Web App, Docker, Docker Compose  
**Værktøjer:** VS Code / Visual Studio, Terminal, Docker Desktop

**Didaktisk fokus:**  
- Skift i mental model: *fra “projekt på min computer” til “applikation i miljø”*  
- Bevidsthed om hvilke trin IDE’er normalt skjuler  
- Forståelse frem for automatisering

---

## Læringsmål

### Viden
- forskellen mellem lokal afvikling og containeriseret afvikling
- hvad et Docker image og en container er
- hvorfor applikationen i containeren er uafhængig af udviklingsmaskinen

### Færdigheder
- oprette en ny ASP.NET Core Web App fra terminalen
- skrive kode i `Program.cs`
- bygge applikationen til deployment (`dotnet publish`)
- bruge `docker compose` til at starte applikationen

### Kompetencer
- forklare hele vejen fra `dotnet new` til kørende container
- opdage og forklare fejlkilder relateret til miljø og runtime
- argumentere imod “det virker på min computer” som kvalitetskriterium

---

## Del 0 – Opret projekt og mappe

```bash
mkdir HelloWorld
cd HelloWorld
```

Opret en ny ASP.NET Core Web App:

```bash
dotnet new webapp -n HelloWorld
cd HelloWorld
```

---

## Del 1 – Kode i Program.cs

Åbn `Program.cs` og **erstat indholdet** med følgende:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World fra Docker 👋");

app.Run();
```

Start applikationen lokalt:

```bash
dotnet run
```

Test i browseren.

**Pædagogisk intention:**  
At sikre at alle har **samme, synlige funktionalitet**, før containerisering.

---

## Del 2 – Build til deployment

```bash
ctrl + c
dotnet publish -c Release
```

Undersøg mappen:

```text
bin/Release/net8.0/publish/ #(husk at vælge den rigtige version)
```

---

## Del 3 – Dockerfile

Opret en fil `Dockerfile` i projektmappen (`HelloWorld/HelloWorld`).

```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 #(vælge den rigtige version)
WORKDIR /app
EXPOSE 8080
COPY bin/Release/net8.0/publish .
ENTRYPOINT ["dotnet", "HelloWorld.dll"]
```

---

## Del 4 – docker-compose.yml

Opret en fil **ved siden af Dockerfile** med navnet `docker-compose.yml`:

```yaml
services:
  web:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
```

**Pædagogisk intention:**  
At vise at **kørsel og konfiguration er adskilt fra kode**.

---

## Del 5 – Kør via docker compose

```bash
docker compose up --build
```

Åbn browseren:

```text
http://localhost:8080
```

Stop igen med `Ctrl+C`.

---

## Del 6 – Ændr koden (bevidst brud)

1. Ret teksten i `Program.cs`  
2. Gem filen  
3. Refresh browseren

### Observation
Ændringen slår **ikke** igennem.

Gentag nu hele flowet:

```bash
dotnet publish -c Release
docker compose up --build
```

**Begreb:** Immutable builds

---

## Aflevering & evaluering

Ingen kodeaflevering.

Succes kriterium er, at gruppen mundtligt kan forklare:
- hvor koden kører lokalt vs i container
- hvorfor compose ikke ser kildekoden direkte
- hvorfor rebuild er nødvendig

---

## Mulige udvidelser (ikke del af øvelsen)
- multi-stage Dockerfile
- docker compose med flere services
- dev vs prod images
## Del 7 – Multi-stage build

Indtil nu har vi kørt `dotnet publish` manuelt, før vi byggede imaget. Det er skrøbeligt: hvis nogen glemmer trinnet, eller publisher til en forkert mappe, fejler build'et. En **multi-stage build** løser det ved at lade Docker selv stå for både build og runtime, i to adskilte "stages" i samme Dockerfile.

Erstat indholdet af `Dockerfile` med:

```Dockerfile
# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HelloWorld.dll"]
```

Kør igen:

```bash
docker compose up --build
```

### Observation
Du behøver ikke længere køre `dotnet publish` manuelt – det sker inde i build-processen.

**Pædagogisk intention:**  
At vise at build-miljøet (SDK) og runtime-miljøet (ASP.NET runtime) kan være to forskellige images, og at det endelige image kun indeholder det færdigbyggede resultat – ikke kildekode eller SDK-værktøjer. Det gør imaget mindre og build'et reproducerbart uafhængigt af hvad der ligger lokalt på udviklingsmaskinen.

**Refleksionsspørgsmål:**  
- Hvorfor er det et problem, hvis SDK'en (byggeværktøjerne) er med i det image, der kører i produktion?
- Hvad sker der, hvis du sletter din lokale `bin/`-mappe og kører `docker compose up --build` igen?

---

## Del 8 – Flere services i docker-compose

En applikation kører sjældent alene. Vi tilføjer nu en database som en selvstændig service, og lader web-applikationen afhænge af den.

Udvid `docker-compose.yml`:

```yaml
services:
  web:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
    depends_on:
      - db

  db:
    image: postgres:16
    environment:
      - POSTGRES_PASSWORD=example
    ports:
      - "5432:5432"
```

Kør:

```bash
docker compose up --build
```

### Observation
Der startes nu to containere. `web` kan ikke tilgå databasen via `localhost` – i stedet skal servicenavnet `db` bruges som hostname (fx `Host=db;Password=example`), fordi containerne kører på et fælles, internt Docker-netværk oprettet automatisk af compose.

**Pædagogisk intention:**  
At vise hvordan containere finder hinanden via servicenavne i stedet for IP-adresser eller `localhost`, og at introducere `depends_on` – samt dens begrænsning: det garanterer kun *startrækkefølge*, ikke at databasen faktisk er klar til at modtage forbindelser.

**Refleksionsspørgsmål:**  
- Hvorfor virker `localhost:5432` ikke fra web-containeren, selvom porten er mappet?
- Hvad er forskellen på at en container er "startet" og at den er "klar"?

---

## Del 9 – Dev vs. prod

Til sidst ser vi på, hvordan man kan holde en udviklingsopsætning (med hurtig feedback) adskilt fra en produktionsopsætning (med immutable builds, som i Del 6).

Opret en ny fil `docker-compose.override.yml` ved siden af den eksisterende:

```yaml
services:
  web:
    build:
      context: .
      target: build
    volumes:
      - .:/src
    command: ["dotnet", "watch", "run", "--urls=http://+:8080"]
```

`docker compose` bruger automatisk `docker-compose.yml` sammen med `docker-compose.override.yml`, når filen findes i samme mappe. Kør:

```bash
docker compose up --build
```

Ret nu i `Program.cs` igen, gem, og refresh browseren – uden at køre `dotnet publish` eller `docker compose up --build` igen.

### Observation
Ændringen slår igennem med det samme, fordi kildekoden er mountet direkte ind i containeren (`volumes`), og `dotnet watch` genstarter appen automatisk. Dette er stik modsat opførslen i Del 6.

**Pædagogisk intention:**  
At gøre eksplicit, at "dev-oplevelsen" (hurtig feedback, live reload) og "prod-oplevelsen" (immutable, reproducerbare builds) er to bevidste, forskellige konfigurationer – ikke noget der bare "sker automatisk". Compose-filer kan lagdeles, så man ikke behøver duplikere hele opsætningen.

**Refleksionsspørgsmål:**  
- Hvilke af de to opsætninger (Del 6 vs. Del 9) ville du bruge på en produktionsserver, og hvorfor?
- Hvad er risikoen ved at bruge dev-opsætningen (med volumes) i produktion?

---
