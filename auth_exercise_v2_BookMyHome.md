# Øvelse: Authentication Microservice med YARP, MySQL og Blazor GUI

## Intro (casen)

Alt kører fint i jeres system. Men jeres Product Owner (PO) kommer tilbage fra konference:

> "Systemet skal udvides! Vi skal have **rigtig authentication** – og det skal selvfølgelig laves som en selvstændig microservice, så vi kan skalere det uafhængigt!"

Og for at gøre det sjovere:

> "Alt login skal rutes igennem Gateway’en – og I skal selvfølgelig også bygge en GUI i Blazor til det."

---

## Hvorfor MySQL og Minimal API?

- **Hvorfor MySQL?**  
  Mange af jer har arbejdet med MS SQL. Vi bruger MySQL her, fordi:  
  - Den kører meget nemt i Docker.  
  - Den er meget udbredt i microservice-arkitektur.  
  - Tænk på MySQL som "kusinen" til MS SQL – de kan meget af det samme, men bruger lidt andre nøgleord (fx `AUTO_INCREMENT` i stedet for `IDENTITY`).  

- **Hvorfor Minimal API?**  
  I har arbejdet med klassiske REST Controllers. Minimal API er bare en **kortere måde** at skrive det samme på:  
  - `app.MapGet("/users", ...)` svarer til en Controller med `[HttpGet("/users")]`.  
  - Det er REST – bare med mindre "boilerplate".  
  - Læringsmålet er, at I ser en ny stil og kan genkende mønstrene.  

---

## Arkitektur

```text
Browser (Blazor GUI)
        |
        v
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

### Step 1 – Database (MySQL)

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

### Step 2 – AuthenticationService

**Metadata**  
- *Hvorfor vigtigt*: Isolation af domæne (login) i microservice.  
- *Færdigheder*: Minimal API i C#, password hashing, DB-adgang.  

`AuthenticationService/Program.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connString = builder.Configuration.GetConnectionString("Default");

// DTO til JSON binding
record UserLogin(string Username, string Password);

string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}

app.MapPost("/register", async (UserLogin input) =>
{
    using var conn = new MySqlConnection(connString);
    await conn.OpenAsync();

    var hash = HashPassword(input.Password);

    var cmd = new MySqlCommand("INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)", conn);
    cmd.Parameters.AddWithValue("@u", input.Username);
    cmd.Parameters.AddWithValue("@p", hash);

    await cmd.ExecuteNonQueryAsync();
    return Results.Ok("User registered!");
});

app.MapPost("/login", async (UserLogin input) =>
{
    using var conn = new MySqlConnection(connString);
    await conn.OpenAsync();

    var cmd = new MySqlCommand("SELECT PasswordHash FROM Users WHERE Username=@u", conn);
    cmd.Parameters.AddWithValue("@u", input.Username);

    var result = await cmd.ExecuteScalarAsync();
    if (result == null) return Results.Unauthorized();

    var storedHash = result.ToString();
    var loginHash = HashPassword(input.Password);

    return storedHash == loginHash ? Results.Ok("Login successful!") : Results.Unauthorized();
});

app.Run();
```

**AuthenticationService.csproj** skal inkludere:

```xml
<ItemGroup>
  <PackageReference Include="MySql.Data" Version="8.3.0" />
</ItemGroup>
```

---

### Step 3 – Gateway (YARP)

**Metadata**  
- *Hvorfor vigtigt*: Central indgang til systemet.  
- *Færdigheder*: Konfiguration af YARP med docker service-navne.  

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

**Gateway.csproj**:

```xml
<ItemGroup>
  <PackageReference Include="Yarp.ReverseProxy" Version="2.1.0" />
</ItemGroup>
```

---

### Step 4 – Docker Compose

**Metadata**  
- *Hvorfor vigtigt*: Samler alle services i ét miljø.  
- *Færdigheder*: Compose filer, netværk, env vars.  

`docker-compose.yml`:

```yaml
services:
  gateway:
    build: ./Gateway
    environment:
      - ASPNETCORE_URLS=http://+:5000
    ports:
      - "8080:5000"
    depends_on:
      - authenticationservice

  authenticationservice:
    build: ./AuthenticationService
    environment:
      - ASPNETCORE_URLS=http://+:5000
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

### Step 5 – Blazor GUI (stub)

**Metadata**  
- *Hvorfor vigtigt*: Visualisering og brugerinddragelse.  
- *Færdigheder*: Blazor, HttpClient integration, form håndtering.  

`BlazorGui/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped(sp => new HttpClient());
var app = builder.Build();
app.Run();
```

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

### Step 6 – Test

**Metadata**  
- *Hvorfor vigtigt*: Verifikation af integrationen.  
- *Færdigheder*: REST test med curl/Postman + GUI test.  

1. Kør `docker compose up --build`.  
2. Kald:  
   - `POST http://localhost:8080/auth/register` med JSON `{ "username": "alice", "password": "secret" }`.  
   - `POST http://localhost:8080/auth/login` med JSON `{ "username": "alice", "password": "secret" }`.  
3. Start Blazor GUI og test via `/register` og `/login`.  

---

## Læringsmål

- Bygge en **Authentication microservice** i C#.  
- Opsætte **MySQL i Docker** og forstå forskelle fra MS SQL.  
- Bruge **Minimal API** som REST.  
- Styre indgangen til systemet via **YARP Gateway**.  
- Integrere en **Blazor GUI** med en backend-service.  
- Forstå hvordan **Docker netværk** gør services tilgængelige via navne.  
- Øve **end-to-end integrationstest**.  
