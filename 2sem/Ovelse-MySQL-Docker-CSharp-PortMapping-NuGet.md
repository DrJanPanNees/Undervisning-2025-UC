# Øvelse: MySQL med Docker, C# Transactions og Port Mapping

## Filstruktur
Opret en mappe, fx `DockerSQL`, med følgende indhold:

```
DockerSQL/
├── docker-compose.yml
├── init.sql
└── Program.cs   (ligger i dit C# Console App projekt)
```

---

## Metadata

**Fag:** Teknologi 2 (3. semester, Datamatiker)  

**Hvorfor relevant?**
- *Transaktioner:* Transaktioner sikrer konsistens i databaser. Hvis én del af en proces fejler, kan man rulle alt tilbage (ROLLBACK). Det bruges fx ved bankoverførsler, e-handel og ordrehåndtering.
- *Docker:* Docker gør det nemt at opsætte en database eller service uden at installere den direkte på din computer. Alle i gruppen kan få præcis samme opsætning, så I undgår problemer med “det virker på min maskine”.
- *Relevans for 2. semester projektet:*  
  - Opsætte en fælles MySQL database, som hele gruppen kan connecte til.  
  - Teste forskellige versioner af databasen uden at ødelægge jeres lokale installation.  
  - Bruge containere til både backend (C# API) og database, så projektet bliver nemmere at køre på tværs af computere.  
  - Forberede jer på, hvordan software i virkeligheden deployes i virksomheder (ofte med Docker/Kubernetes).  

**Læringsmål:**
- **Viden:** Forstå begreberne COMMIT og ROLLBACK i en database.  
- **Færdighed:** Opsætte en database i Docker og afvikle transaktioner fra C#.  
- **Kompetence:** Analysere hvornår transaktioner giver værdi, og hvordan Docker kan sikre ensartede miljøer i projektarbejde.  

---

## 1) Installer Docker Desktop
Download Docker Desktop og kør `docker --version` for at teste.

[Docker Desktop download](https://www.docker.com/products/docker-desktop/)

---

## 2) docker-compose.yml
Opret filen **docker-compose.yml**:

```yaml
version: "3.9"
services:
  mysql:
    image: mysql:8.0
    container_name: mysql-demo
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: rootpass
      MYSQL_DATABASE: demo
      MYSQL_USER: demo
      MYSQL_PASSWORD: demopass
    ports:
      - "3307:3306"   # Host-port 3307 → Container-port 3306
    volumes:
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql:ro
```

---

## 3) init.sql
Opret filen **init.sql**:

```sql
CREATE TABLE accounts (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50),
    balance DECIMAL(10,2)
);

INSERT INTO accounts (name, balance)
VALUES ('Alice', 1000.00), ('Bob', 500.00);
```

---

## 4) Start MySQL containeren
I terminalen, gå til mappen med din `docker-compose.yml`, og kør:

```bash
docker compose up -d
```

Tjek status med:

```bash
docker ps
```

---

## 5) Log ind i databasen (valgfrit)
```bash
docker exec -it mysql-demo mysql -u demo -p demo
```

Eksempel på SQL-kommandoer:

```sql
SHOW DATABASES;
USE demo;
SELECT * FROM accounts;
```

---

## 6) Installer NuGet-pakken til MySQL

For at C# kan forbinde til MySQL skal projektet have en database-driver.  
Vi bruger **MySql.Data** (fra Oracle), som passer til eksemplet.

Kør dette i **Package Manager Console** eller **Terminal**:

```bash
dotnet add package MySql.Data
```

Hvis du hellere vil bruge den hurtigere community-version, kan du i stedet installere:

```bash
dotnet add package MySqlConnector
```

> ⚠️ Hvis du bruger `MySqlConnector`, skal du ændre `using MySql.Data.MySqlClient;` til `using MySqlConnector;` i Program.cs.

---

## 7) C# Transaktioner (COMMIT / ROLLBACK)

Eksempel på **Program.cs**:

```csharp
using System;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        var connString = "Server=localhost;Port=3307;Database=demo;Uid=demo;Pwd=demopass;";

        using var conn = new MySqlConnection(connString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Alice sender 200 til Bob
            var withdrawCmd = new MySqlCommand(
                "UPDATE accounts SET balance = balance - 200 WHERE name = 'Alice';",
                conn, transaction);
            withdrawCmd.ExecuteNonQuery();

            var depositCmd = new MySqlCommand(
                "UPDATE accounts SET balance = balance + 200 WHERE name = 'Bob';",
                conn, transaction);
            depositCmd.ExecuteNonQuery();

            // Tjek resultat inden commit
            var checkCmd = new MySqlCommand("SELECT name, balance FROM accounts;", conn, transaction);
            using var reader = checkCmd.ExecuteReader();
            Console.WriteLine("=== Midlertidige værdier (før commit) ===");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["name"]}: {reader["balance"]}");
            }

            // Commit ændringer
            transaction.Commit();
            Console.WriteLine(">>> Transaction committed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("!!! Fejl skete, laver rollback:");
            Console.WriteLine(ex.Message);
            transaction.Rollback();
        }
    }
}
```

---

## 8) Visualisering af Port Mapping

```
+-------------------+        TCP/IP (localhost:port)        +---------------------+
|                   | ------------------------------------> |                     |
|   C# Program      |                                       |   Docker Container  |
|   (Program.cs)    | <------------------------------------ |   mysql:8.0         |
|                   |          SQL-respons (resultat)       |                     |
+-------------------+                                       |   Lytter på 3306    |
         |                                                   +---------------------+
         |
         |  (konfigureret connection string i C#)
         |  Server=localhost;Port=3307;Database=demo;
         |  Uid=demo;Pwd=demopass;
         v
+-------------------+
|   Lokal port 3307 |  ----->  NAT / Port mapping  ----->   Container port 3306
+-------------------+
```

---

## Øvelsesopgaver

1. **Standardforbindelse:** Brug connection string med `Port=3307` og kør programmet.  
2. **Rollback-test:** Tilføj en SQL-fejl i anden UPDATE for at se rollback i praksis.  
3. **Ændr porten:** Ret docker-compose til `"3308:3306"` og opdater connection string i C#. Test at det stadig virker.  
4. **Udvid tabellen:** Tilføj flere konti i `init.sql` og lav en transaktion mellem tre personer.  
5. **Opsaml refleksion:** Hvornår giver transaktioner mest mening i et rigtigt system? Hvordan hjælper Docker jer i projektarbejdet?  

---

## Succeskriterier
- Programmet kører uden fejl.  
- Efter første commit: **Alice = 800, Bob = 700**.  
- Ved fejl → rollback → værdierne ændres ikke.  
- Alle i gruppen kan reproducere opsætningen via Docker.  
