# Øvelse: Authentication Microservice med YARP, MySQL og Blazor GUI

## Intro (casen)

Alt kører fint i jeres system. I har allerede services og måske en tidlig GUI.  
Men… jeres Product Owner (PO) har været til konference, og kommer tilbage med en *game changer*:

> "Systemet skal udvides! Vi kan ikke længere have brugere uden login.  
> Vi skal have **rigtig authentication** – og det skal selvfølgelig laves som en selvstændig microservice,  
> så vi kan skalere det uafhængigt!"

PO kigger alvorligt på jer og tilføjer:

> "Og husk – alt login skal rutes igennem Gateway’en, for vi skal kunne styre adgang ét sted.  
> Nå ja… I skal selvfølgelig også bygge en GUI i Blazor til det."

Det er nu jeres opgave at udvide jeres setup med en **AuthenticationService**, der understøtter `/register` og `/login`.  
Brugerne skal gemmes i en MySQL-database med password-hash.  
Gatewayen (YARP) skal rute alle `/auth/*` requests til den nye service.  
Jeres Blazor GUI skal så integrere mod gateway’en.

---

## Arkitektur

```text
                 ┌───────────────────┐
                 │   Gateway (YARP)  │
                 │   Routes: /auth/* │
                 └─────────┬─────────┘
                           │
                           v
              ┌────────────────────────┐
              │ Authentication Service │
              │   /register   /login   │
              └─────────┬─────────────┘
                        │
                        v
              ┌────────────────────────┐
              │   MySQL (usersdb)      │
              │   Table: Users         │
              │   - Id                 │
              │   - Username           │
              │   - PasswordHash       │
              └────────────────────────┘
```

---

## Filstruktur (formodet)

```text
/AuthDemo
├── docker-compose.yml
│
├── Gateway
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Gateway.csproj
│   └── Dockerfile
│
├── AuthenticationService
│   ├── Program.cs
│   ├── AuthenticationService.csproj
│   ├── appsettings.json
│   ├── init.sql
│   └── Dockerfile
│
└── BlazorGui
    ├── Pages
    │   ├── Login.razor
    │   └── Register.razor
    ├── Program.cs
    └── BlazorGui.csproj
```

---

## Øvelsen

### Step 1 – Forstå kravet  
**Metadata**  
- *Hvorfor vigtigt*: Realistisk kravændring – PO ændrer systemet.  
- *Færdigheder*: Kravanalyse, opdatering af systemarkitektur.  

Opgave: Beskriv hvad der ændrer sig i jeres arkitektur, når login flyttes til en microservice.

---

### Step 2 – Opsæt database (MySQL)  
**Metadata**  
- *Hvorfor vigtigt*: Brugere skal gemmes sikkert.  
- *Færdigheder*: Init scripts, Docker Compose med database.  

`AuthenticationService/init.sql`:

```sql
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(200) NOT NULL
);
```

---

### Step 3 – AuthenticationService (C# Minimal API)  
**Metadata**  
- *Hvorfor vigtigt*: Service isolation + Domain Centric Architecture.  
- *Færdigheder*: Minimal API i C#, hash af passwords, CRUD mod MySQL.  

`AuthenticationService/Program.cs` (stub):

```csharp
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connString = builder.Configuration.GetConnectionString("Default");

// Hash funktion (simpel demo – brug evt. BCrypt i stedet)
string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}

app.MapPost("/register", async (string username, string password) =>
{
    using var conn = new MySqlConnection(connString);
    await conn.OpenAsync();

    var hash = HashPassword(password);

    var cmd = new MySqlCommand("INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)", conn);
    cmd.Parameters.AddWithValue("@u", username);
    cmd.Parameters.AddWithValue("@p", hash);

    await cmd.ExecuteNonQueryAsync();

    return Results.Ok("User registered!");
});

app.MapPost("/login", async (string username, string password) =>
{
    using var conn = new MySqlConnection(connString);
    await conn.OpenAsync();

    var cmd = new MySqlCommand("SELECT PasswordHash FROM Users WHERE Username=@u", conn);
    cmd.Parameters.AddWithValue("@u", username);

    var result = await cmd.ExecuteScalarAsync();
    if (result == null) return Results.Unauthorized();

    var storedHash = result.ToString();
    var loginHash = HashPassword(password);

    return storedHash == loginHash ? Results.Ok("Login successful!") : Results.Unauthorized();
});

app.Run();
```

---

### Step 4 – Gateway (YARP)  
**Metadata**  
- *Hvorfor vigtigt*: Gateway styrer rute og sikkerhed.  
- *Færdigheder*: YARP-konfiguration i `appsettings.json`.  

`Gateway/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "authRoute": {
        "ClusterId": "authCluster",
        "Match": { "Path": "/auth/{**catch-all}" }
      }
    },
    "Clusters": {
      "authCluster": {
        "Destinations": {
          "auth": { "Address": "http://authenticationservice:5000" }
        }
      }
    }
  }
}
```

---

### Step 5 – Docker Compose  
**Metadata**  
- *Hvorfor vigtigt*: Hele systemet skal kunne køre samlet.  
- *Færdigheder*: Compose med flere services og netværk.  

`docker-compose.yml`:

```yaml
services:
  gateway:
    build: ./Gateway
    ports:
      - "8080:5000"
    depends_on:
      - authenticationservice

  authenticationservice:
    build: ./AuthenticationService
    environment:
      - ConnectionStrings__Default=server=usersdb;user=root;password=root;database=usersdb
    depends_on:
      - usersdb

  usersdb:
    image: mysql:8.0
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: usersdb
    volumes:
      - ./AuthenticationService/init.sql:/docker-entrypoint-initdb.d/init.sql
```

---

### Step 6 – Blazor GUI (stub)  
**Metadata**  
- *Hvorfor vigtigt*: Visualisering og integration.  
- *Færdigheder*: Bygge GUI, integration mod REST API.  

`BlazorGui/Pages/Register.razor`:

```razor
@page "/register"
@inject HttpClient Http

<h3>Register</h3>

<input @bind="username" placeholder="Username" />
<input @bind="password" placeholder="Password" type="password" />
<button @onclick="Register">Register</button>

<p>@message</p>

@code {
    string username = "";
    string password = "";
    string message = "";

    async Task Register()
    {
        var response = await Http.PostAsJsonAsync("http://localhost:8080/auth/register", new { username, password });
        message = await response.Content.ReadAsStringAsync();
    }
}
```

`BlazorGui/Pages/Login.razor`:

```razor
@page "/login"
@inject HttpClient Http

<h3>Login</h3>

<input @bind="username" placeholder="Username" />
<input @bind="password" placeholder="Password" type="password" />
<button @onclick="Login">Login</button>

<p>@message</p>

@code {
    string username = "";
    string password = "";
    string message = "";

    async Task Login()
    {
        var response = await Http.PostAsJsonAsync("http://localhost:8080/auth/login", new { username, password });
        message = await response.Content.ReadAsStringAsync();
    }
}
```

---

### Step 7 – Test  
**Metadata**  
- *Hvorfor vigtigt*: Verifikation af end-to-end flow.  
- *Færdigheder*: Brug af REST-clients (curl/Postman/Blazor GUI).  

Opgave:  
- `POST http://localhost:8080/auth/register` med JSON `{ "username": "alice", "password": "secret" }`.  
- `POST http://localhost:8080/auth/login` med JSON `{ "username": "alice", "password": "secret" }`.  
- Tjek at login virker, og at DB har brugerdata.  
- Test Blazor GUI (gå til `/register` og `/login`).  

---

## Læringsmål

- Forstå hvordan **nye krav** kan påvirke arkitektur.  
- Kunne bygge en **Authentication microservice** i C#.  
- Opsætte **MySQL database i Docker** og koble den til en service.  
- Bruge **YARP Gateway** til at styre routing.  
- Integrere en **Blazor GUI** mod en microservice via gateway.  
- Øve **end-to-end integrationstest**.  
