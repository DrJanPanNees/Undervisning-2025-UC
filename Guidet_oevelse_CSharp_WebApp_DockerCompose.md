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
