# Øvelse: MySQL med Docker og C# Transactions

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
- **Fag:** Teknologi 2 (3. semester, Datamatiker)  
- **Hvorfor relevant?**  
  Transaktioner sikrer konsistens i databaser. Docker giver samme miljø på tværs af computere (“det virker på min maskine”-problemet).  
  Begge dele kan bruges direkte i semesterprojektet.  
- **Læringsmål:**  
  Forstå COMMIT og ROLLBACK, opsætte database i Docker, afvikle transaktioner fra C#.

---

## 1) Installer Docker Desktop
Hent Docker Desktop: [docker.com](https://www.docker.com/products/docker-desktop/)  
Test installation med:

```bash
docker --version
```

---

## 2) Opret `docker-compose.yml`
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
      - "3306:3306"
    volumes:
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql:ro
```

---

## 3) Opret `init.sql`
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

## 4) Start databasen
```bash
docker compose up -d
docker ps
```

---

## 5) Test databasen (valgfrit)
```bash
docker exec -it mysql-demo mysql -u demo -pdemo demo
SELECT * FROM accounts;
```

---

## 6) C# – Program.cs (Console App + NuGet: MySql.Data)
```csharp
using System;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        var connString = "Server=localhost;Port=3306;Database=demo;Uid=demo;Pwd=demopass;";
        using var conn = new MySqlConnection(connString);
        conn.Open();

        using var tx = conn.BeginTransaction();
        try
        {
            // Alice -> Bob (200)
            var withdraw = new MySqlCommand(
                "UPDATE accounts SET balance = balance - 200 WHERE name = 'Alice';", conn, tx);
            withdraw.ExecuteNonQuery();

            var deposit = new MySqlCommand(
                "UPDATE accounts SET balance = balance + 200 WHERE name = 'Bob';", conn, tx);
            deposit.ExecuteNonQuery();

            tx.Commit();
            Console.WriteLine(">>> Transaction committed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("!!! Fejl, laver rollback:");
            Console.WriteLine(ex.Message);
            tx.Rollback();
        }
    }
}
```

---

## Succeskriterier
- Programmet kører uden fejl.  
- Efter første kørsel: **Alice = 800, Bob = 700 (commit)**.  
- Ved en bevidst fejl i SQL rulles transaktionen tilbage (**rollback**).  
- I kan alle køre samme opsætning via Docker på jeres maskiner.  
