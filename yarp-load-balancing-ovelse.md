# ⚖️ Øvelse 2: Load balancing med YARP foran 3 Nginx-containere

I den forrige øvelse rutede YARP trafik til **forskellige** websites (`/site1` og `/site2`).
I denne øvelse bruger du YARP til at **fordele trafikken ligeligt mellem flere identiske servere**, og du undersøger, hvad der sker, når en af dem går ned.

---

## 🎯 Læringsmål

- Forstå forskellen på **routing** (hvem skal have requesten?) og **load balancing** (hvilken af flere ens servere?).
- Konfigurere **ét cluster med flere destinations** i YARP.
- Bruge og sammenligne **load balancing-politikker** (`RoundRobin`, `Random`, `LeastRequests`).
- Se hvad der sker, når en server går ned, og hvordan **health checks** løser problemet.
- Bruge **session affinity**, så en bruger bliver ved med at ramme den samme server.

## ✅ Forudsætninger

- Du har lavet øvelse 1 (YARP som reverse proxy), eller du kender Docker Compose og `dotnet new web`.
- Docker Desktop og .NET SDK 8.0 eller nyere er installeret.

---

## 📂 Projektstruktur

```
reverseproxy-lb/
├── docker-compose.yml
├── nginx/
│   └── default.conf
├── html1/
│   └── index.html
├── html2/
│   └── index.html
├── html3/
│   └── index.html
└── YarpLoadBalancer/
    ├── YarpLoadBalancer.csproj
    ├── Program.cs
    └── appsettings.json
```

---

## 0️⃣ Opstart af projektet

```bash
mkdir reverseproxy-lb && cd reverseproxy-lb
mkdir nginx html1 html2 html3

mkdir YarpLoadBalancer && cd YarpLoadBalancer
dotnet new web
dotnet add package Yarp.ReverseProxy
cd ..
```

> 💡 `dotnet new web` genererer selv `Program.cs` og `appsettings.json`.
> Du skal **erstatte indholdet** af begge filer med koden herunder.

---

## 1️⃣ Nginx-konfiguration

Alle tre containere deler samme konfiguration. Den gør to ting, som er vigtige for øvelsen:

- `Cache-Control: no-store` forhindrer browseren i at vise en cachet side. Ellers ville du ikke kunne se, at svaret kommer fra forskellige servere.
- `X-Backend` er en header, der fortæller, hvilken server der svarede.

**nginx/default.conf**
```nginx
server {
    listen 80;
    server_name _;

    root /usr/share/nginx/html;
    index index.html;

    add_header Cache-Control "no-store" always;
    add_header X-Backend $hostname always;
}
```

---

## 2️⃣ Docker Compose

**docker-compose.yml**
```yaml
services:
  nginx1:
    image: nginx:alpine
    container_name: nginx1
    hostname: nginx1
    ports:
      - "5001:80"
    volumes:
      - ./nginx/default.conf:/etc/nginx/conf.d/default.conf:ro
      - ./html1:/usr/share/nginx/html:ro

  nginx2:
    image: nginx:alpine
    container_name: nginx2
    hostname: nginx2
    ports:
      - "5002:80"
    volumes:
      - ./nginx/default.conf:/etc/nginx/conf.d/default.conf:ro
      - ./html2:/usr/share/nginx/html:ro

  nginx3:
    image: nginx:alpine
    container_name: nginx3
    hostname: nginx3
    ports:
      - "5003:80"
    volumes:
      - ./nginx/default.conf:/etc/nginx/conf.d/default.conf:ro
      - ./html3:/usr/share/nginx/html:ro
```

---

## 3️⃣ HTML-sider

De tre sider er ens, bortset fra navn og baggrundsfarve. Så kan du **se** hvilken server, der svarer.

**html1/index.html**
```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>Server 1</title>
</head>
<body style="background:#cfe8ff; font-family:sans-serif; text-align:center; padding-top:80px;">
  <h1>Server 1 (nginx1)</h1>
  <p>Port 5001</p>
</body>
</html>
```

**html2/index.html**
```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>Server 2</title>
</head>
<body style="background:#d4f5d4; font-family:sans-serif; text-align:center; padding-top:80px;">
  <h1>Server 2 (nginx2)</h1>
  <p>Port 5002</p>
</body>
</html>
```

**html3/index.html**
```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>Server 3</title>
</head>
<body style="background:#ffe3c2; font-family:sans-serif; text-align:center; padding-top:80px;">
  <h1>Server 3 (nginx3)</h1>
  <p>Port 5003</p>
</body>
</html>
```

---

## 4️⃣ YARP-konfiguration (Round Robin)

Her er der kun **én route** og **ét cluster**, men clusteret har **tre destinations**.

**YarpLoadBalancer/appsettings.json**
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
      "webRoute": {
        "ClusterId": "webCluster",
        "Match": {
          "Path": "/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "webCluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "server1": { "Address": "http://localhost:5001/" },
          "server2": { "Address": "http://localhost:5002/" },
          "server3": { "Address": "http://localhost:5003/" }
        }
      }
    }
  }
}
```

**Sådan virker det:**

- Routen fanger **alle** requests (`/{**catch-all}`) og sender dem til `webCluster`. Der er ingen `PathRemovePrefix`, fordi vi ikke bruger prefix her.
- `LoadBalancingPolicy` bestemmer, hvilken destination i clusteret der får næste request.
- `RoundRobin` tager destinations på skift: 1, 2, 3, 1, 2, 3, …

---

## 5️⃣ YARP C# Program

**YarpLoadBalancer/Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run("http://localhost:4000");
```

---

## 6️⃣ Kørsel

1. Start Nginx-containerne fra **rodmappen** (`reverseproxy-lb/`):
   ```bash
   docker compose up -d
   docker compose ps
   ```

2. Test hver server direkte:
   - <http://localhost:5001> → blå side
   - <http://localhost:5002> → grøn side
   - <http://localhost:5003> → orange side

3. Start YARP fra **YarpLoadBalancer-mappen**:
   ```bash
   cd YarpLoadBalancer
   dotnet run
   ```

---

## 7️⃣ Del A: Se Round Robin i aktion

### I browseren

Åbn <http://localhost:4000/> og tryk `F5` flere gange. Baggrundsfarven skal skifte mellem blå, grøn og orange.

### I terminalen

**Bash / macOS / Linux:**
```bash
for i in $(seq 1 9); do curl -s http://localhost:4000/ | grep "<h1>"; done
```

**Windows PowerShell:**
```powershell
1..9 | ForEach-Object { curl.exe -s http://localhost:4000/ | Select-String "<h1>" }
```

> 💡 I PowerShell er `curl` et alias for `Invoke-WebRequest`. Derfor skal du skrive `curl.exe`.

Du skal se, at serverne kommer på skift:

```
<h1>Server 2 (nginx2)</h1>
<h1>Server 3 (nginx3)</h1>
<h1>Server 1 (nginx1)</h1>
<h1>Server 2 (nginx2)</h1>
...
```

Rækkefølgen kan starte forskelligt, men den skal gentage sig i et fast mønster.

**Spørgsmål til refleksion:**
1. Hvor mange requests får hver server ud af 9?
2. Hvad ville der ske med fordelingen, hvis en server var meget langsommere end de andre?

---

## 8️⃣ Del B: Hvad sker der, når en server går ned?

1. Stop én af serverne, mens YARP kører:
   ```bash
   docker stop nginx2
   ```

2. Kør test-løkken fra Del A igen (eller tryk `F5` i browseren).

Hver tredje request fejler nu med **502 Bad Gateway**, fordi YARP stadig forsøger at sende trafik til den døde server. YARP ved ikke, at serveren er nede, så længe vi ikke har bedt den om at holde øje.

3. Start serveren igen:
   ```bash
   docker start nginx2
   ```

---

## 9️⃣ Del C: Active health checks

Med **active health checks** pinger YARP selv alle destinations med jævne mellemrum og holder op med at sende trafik til dem, der ikke svarer.

Udvid `webCluster` i `appsettings.json` med `HealthCheck` og `Metadata`:

```json
"webCluster": {
  "LoadBalancingPolicy": "RoundRobin",
  "HealthCheck": {
    "Active": {
      "Enabled": "true",
      "Interval": "00:00:05",
      "Timeout": "00:00:02",
      "Policy": "ConsecutiveFailures",
      "Path": "/"
    }
  },
  "Metadata": {
    "ConsecutiveFailuresHealthPolicy.Threshold": "2"
  },
  "Destinations": {
    "server1": { "Address": "http://localhost:5001/" },
    "server2": { "Address": "http://localhost:5002/" },
    "server3": { "Address": "http://localhost:5003/" }
  }
}
```

**Hvad betyder det?**

| Indstilling | Betydning |
|---|---|
| `Interval` | Hvor ofte YARP tjekker hver destination (her hvert 5. sekund). |
| `Timeout` | Hvor længe YARP venter på svar, før tjekket tæller som fejlet. |
| `Policy: ConsecutiveFailures` | En destination markeres usund efter et antal fejl i træk. |
| `Threshold` | Antal fejl i træk (her 2). |
| `Path` | Hvilken sti YARP kalder på destinationen. |

**Test:**

1. Genstart YARP (`Ctrl + C`, derefter `dotnet run`).
2. Kør test-løkken, så du kan se, at alle tre servere svarer.
3. Kør `docker stop nginx2`.
4. Kør test-løkken med få sekunders mellemrum. Du får måske et par `502`-fejl, men efter ca. 10 sekunder (2 fejlede tjek med 5 sekunders interval) forsvinder de, og trafikken fordeles kun mellem server 1 og 3.
5. Kør `docker start nginx2`. Efter næste vellykkede tjek kommer server 2 automatisk tilbage i rotationen.

**Spørgsmål til refleksion:**
1. Hvorfor kan man stadig få nogle få fejl, selv med health checks?
2. Hvad er afvejningen ved at vælge et meget kort `Interval`?

---

## 🔟 Del D: Andre load balancing-politikker

Skift `LoadBalancingPolicy` i `appsettings.json`, genstart YARP, og kør test-løkken igen.

| Politik | Opførsel |
|---|---|
| `RoundRobin` | Destinations på skift. |
| `Random` | Tilfældig destination for hver request. |
| `LeastRequests` | Destinationen med færrest igangværende requests. |
| `PowerOfTwoChoices` | Vælger to tilfældige destinations og tager den med færrest requests. |
| `FirstAlphabetical` | Altid den første sunde destination (alfabetisk efter navn). Bruges til failover. |

**Opgaver:**
1. Prøv `Random` med 30 requests. Er fordelingen helt ens? Hvorfor/hvorfor ikke?
2. Prøv `FirstAlphabetical`. Hvilken server får al trafik? Stop den, og se hvad der sker (med health checks slået til).
3. Hvorfor ligner `LeastRequests` `RoundRobin` i denne øvelse? Hvornår ville de opføre sig forskelligt?

---

## 1️⃣1️⃣ Del E: Session affinity (sticky sessions)

Nogle applikationer gemmer data i hukommelsen på serveren (fx login-session eller en indkøbskurv). Så skal en bruger blive ved med at ramme **den samme** server. Det kaldes **session affinity**.

Tilføj `SessionAffinity` til `webCluster` (sæt `LoadBalancingPolicy` tilbage til `RoundRobin`):

```json
"SessionAffinity": {
  "Enabled": true,
  "Policy": "Cookie",
  "FailurePolicy": "Redistribute",
  "AffinityKeyName": ".Yarp.Affinity"
}
```

- `Policy: Cookie` får YARP til at sætte en cookie, der husker, hvilken destination brugeren fik første gang.
- `FailurePolicy: Redistribute` betyder, at hvis den valgte server er nede, vælger YARP en ny.

**Test i browseren:** Åbn <http://localhost:4000/> og tryk `F5` flere gange. Du bliver nu ved med at se den samme farve.
Åbn derefter et **privat vindue** (uden cookie). Du får muligvis en anden server.

**Test i terminalen med og uden cookie:**

Bash:
```bash
# Uden cookie: serverne skifter
for i in $(seq 1 6); do curl -s http://localhost:4000/ | grep "<h1>"; done

# Med cookie-fil: samme server hver gang
for i in $(seq 1 6); do curl -s -c jar.txt -b jar.txt http://localhost:4000/ | grep "<h1>"; done
```

PowerShell:
```powershell
# Uden cookie: serverne skifter
1..6 | ForEach-Object { curl.exe -s http://localhost:4000/ | Select-String "<h1>" }

# Med cookie-fil: samme server hver gang
1..6 | ForEach-Object { curl.exe -s -c jar.txt -b jar.txt http://localhost:4000/ | Select-String "<h1>" }
```

**Failover-test:** Hold den sticky session kørende, og stop den server, du er "låst" til (`docker stop nginxX`). Med health checks slået til flyttes du til en anden server efter få sekunder.

**Spørgsmål til refleksion:**
1. Hvorfor er session affinity en dårlig idé, hvis serveren gemmer data i hukommelsen, og den går ned?
2. Hvilke alternativer findes til at gemme sessionsdata, så man slet ikke har brug for sticky sessions?

---

## 🛠️ Fejlfinding

| Problem | Mulig årsag og løsning |
|---|---|
| Jeg ser altid den samme server i browseren | Du har slået session affinity til (Del E), eller browseren cacher. Tjek at `default.conf` er monteret, og prøv et privat vindue. |
| `502 Bad Gateway` på hver tredje request | Forventet i Del B. Slå health checks til (Del C). |
| `502` på alle requests | Containerne kører ikke. Kør `docker compose ps` og `docker compose up -d`. |
| Siderne viser Nginx' standardside | `default.conf` eller HTML-mapperne er ikke monteret. Tjek stierne i `docker-compose.yml`. |
| Ændringer i `appsettings.json` virker ikke | Stop og start `dotnet run` igen. |
| `port is already allocated` | Port 5001, 5002, 5003 eller 4000 er optaget. Skift porten i både Compose og `appsettings.json`. |

---

## 🚀 Ekstra udfordringer

1. Tilføj en **fjerde** server (`nginx4`, port 5004), og se hvordan fordelingen ændrer sig uden at ændre noget i C#-koden.
2. Aktivér **passive health checks** ved siden af de aktive. Se i YARP's dokumentation under *Destination health checks* hvordan `Passive` og policyen `TransportFailureRate` konfigureres.
3. Lav en destination, der er **langsom** (fx en container med en forsinkelse), og sammenlign `RoundRobin` med `LeastRequests` under belastning. Brug fx `hey`, `bombardier` eller en simpel PowerShell/bash-løkke med flere samtidige requests.
4. Læg YARP selv i en container på samme Docker-netværk som Nginx, og skift destinations til `http://nginx1:80` osv. Overvej, hvorfor Compose-servicenavne virker her, men `localhost` ikke gør.
