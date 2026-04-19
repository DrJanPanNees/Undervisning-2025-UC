# Øvelse: Blazor Server i Docker – uden templates

## Metadata
- **Uddannelse**: Datamatiker
- **Fag**: Systemudvikling / Backend
- **Tema**: ASP.NET Core, Blazor Server, Docker
- **Sværhedsgrad**: Mellem
- **Varighed**: 60–90 minutter
- **Forudsætninger**:
  - Grundlæggende C#
  - Kendskab til ASP.NET Core
  - Grundlæggende Docker (images, containers, ports)
- **Formål**:
  - Forstå hvordan en Blazor Server-app wires op manuelt
  - Forstå betydningen af `Program.cs`
  - Forstå port-styring i Docker
  - Eliminere “magiske” template-valg

---

## Opgavebeskrivelse

I denne øvelse skal du bygge en **Blazor Server-applikation uden at bruge en Blazor-template** og derefter køre den i en Docker-container.

Applikationen skal:
- have al konfiguration i `Program.cs`
- have en minimal, men fungerende Blazor-UI
- køre deterministisk på en fast port i Docker

---

## Del 1 – Projektet

Opret et nyt projekt manuelt.

```bash
dotnet new web -n HelloWorld
cd HelloWorld
```

Projektet **må ikke** oprettes med `dotnet new blazor`.

Kontrollér, at `HelloWorld.csproj` bruger:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
```

---

## Del 2 – Program.cs som arkitekturcentrum

Udskift indholdet af `Program.cs` med en struktur der indeholder:

- Konfiguration (options fra `appsettings.json`)
- Services (state + service)
- Blazor setup
- Minimal API endpoint
- Mapping af Razor components
- Supporting classes nederst i filen

Formålet er, at **Program.cs alene fortæller hele applikationens historie**.

---

## Del 3 – Blazor root component

Opret filen `App.razor` i projektets root.

```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" />
    </Found>
    <NotFound>
        <h3>Siden blev ikke fundet</h3>
    </NotFound>
</Router>
```

Denne fil er applikationens **root component** og er nødvendig, selv om logikken ligger andre steder.

---

## Del 4 – Razor imports

Opret `_Imports.razor` i projektets root.

```razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
```

Uden denne fil vil Razor-komponenter som `Router` og `Found` ikke blive genkendt.

---

## Del 5 – En simpel Blazor-side

Opret mappen `Pages` og filen `Index.razor`.

Siden skal:
- injecte en service
- vise en besked
- reagere på et klik

Formålet er at holde UI tyndt og lade logikken leve i services.

---

## Del 6 – Namespace og binding

Blazor-komponenter bliver compiled til C#-klasser med projektets namespace.

Det betyder, at `App` reelt hedder:

```
HelloWorld.App
```

Sørg derfor for, at `Program.cs` mapper korrekt:

```csharp
app.MapRazorComponents<HelloWorld.App>()
   .AddInteractiveServerRenderMode();
```

---

## Del 7 – Dockerfile

Opret en `Dockerfile` i projektets root.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "HelloWorld.dll"]
```

Applikationen skal lytte på **8080 internt**.

---

## Del 8 – docker-compose.yml

Opret en `docker-compose.yml`.

```yaml
services:
  web:
    build: .
    ports:
      - "8030:8080"
    environment:
      ASPNETCORE_URLS: http://+:8080
```

---

## Del 9 – Kør applikationen i Docker

Byg og kør containeren:

```bash
docker compose build --no-cache
docker compose up
```

Åbn derefter:

```
http://localhost:8030
```

---

## Del 10 – Refleksion

Svar kort på:

1. Hvorfor virker port 5030 nogle gange lokalt, men ikke i Docker?
2. Hvorfor er `ASPNETCORE_URLS=http://+:8080` nødvendig i en container?
3. Hvad er rollen af `Program.cs` i denne arkitektur?
4. Hvorfor opstår der namespace-problemer med `App`?

---

## Aflevering

Aflever:
- Kildekode
- Dockerfile
- docker-compose.yml
- Kort refleksion (½–1 side)
