# Øvelse: LINQ + EF Core + Database (Enten Docker eller MSSQL Server)

## 🎯 Læringsmål
- Opsætte en **database**.  
- Forstå hvordan **Entity Framework Core** kan forbinde C# til en database.  
- Bruge **LINQ** til at forespørge og manipulere data.  
- Bygge en **menu-baseret console app**, der kan vise kunder, produkter, ordrer og tilføje nye produkter.  
- Reflektere over hvordan man selv kan **udvide applikationen** med CRUD-funktioner (oprette/slette kunder og produkter).  

---

## 📂 Filstruktur (baseret på Docker)

```
linq-ef-mysql-demo/
 ├─ db-demo/                     # Docker filer
 │   ├─ docker-compose.yml
 │   └─ init.sql
 │
 └─ LinqDemo/                    # C# Console app
     ├─ LinqDemo.csproj
     ├─ Program.cs
     ├─ DemoContext.cs
     ├─ Kunde.cs
     ├─ Produkt.cs
     ├─ Ordre.cs
     ├─ bin/                     # (genereres af dotnet build/run)
     └─ obj/                     # (genereres af dotnet build/run)
```

---

## 1. Database med Docker Compose (Hvis I vælger at bruge Docker, ellers brug MSSQL SERVER)

**Hvorfor:** Vi bruger Docker for at have en ensartet database, alle kan starte med ét command.  

### `db-demo/docker-compose.yml`
```yaml
version: "3.9"
services:
  mysql:
    image: mysql:8
    container_name: mysql-demo
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: demopass
      MYSQL_DATABASE: demo
    ports:
      - "4000:3306"
    volumes:
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql:ro
```

### `db-demo/init.sql`
```sql
CREATE TABLE Kunde (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Navn VARCHAR(100) NOT NULL,
    Alder INT NOT NULL
);

CREATE TABLE Produkt (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Navn VARCHAR(100) NOT NULL,
    Pris DECIMAL(10,2) NOT NULL
);

CREATE TABLE Ordre (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    KundeId INT NOT NULL,
    ProduktId INT NOT NULL,
    Antal INT NOT NULL,
    FOREIGN KEY (KundeId) REFERENCES Kunde(Id),
    FOREIGN KEY (ProduktId) REFERENCES Produkt(Id)
);

INSERT INTO Kunde (Navn, Alder) VALUES
('Alice', 30),
('Bob', 17);

INSERT INTO Produkt (Navn, Pris) VALUES
('Laptop', 8999.95),
('Mus', 199.95),
('Skærm', 1599.00);

INSERT INTO Ordre (KundeId, ProduktId, Antal) VALUES
(1, 1, 1),
(1, 2, 2),
(2, 3, 1);
```

👉 Start databasen:
```bash
cd db-demo
docker compose up -d
```

> Tjek med `docker ps` at containeren kører, og at port 4000 er åben.  

---

## 2. Opret Console App i C#

**Hvorfor:** Console apps er simple og gode til at lære LINQ + EF Core.  

```bash
dotnet new console -o LinqDemo
cd LinqDemo
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

---

## 3. Model-klasser

### `Kunde.cs`
```csharp
public class Kunde
{
    public int Id { get; set; }
    public string Navn { get; set; }
    public int Alder { get; set; }
}
```

### `Produkt.cs`
```csharp
public class Produkt
{
    public int Id { get; set; }
    public string Navn { get; set; }
    public decimal Pris { get; set; }
}
```

### `Ordre.cs`
```csharp
public class Ordre
{
    public int Id { get; set; }
    public int KundeId { get; set; }
    public int ProduktId { get; set; }
    public int Antal { get; set; }

    public Kunde Kunde { get; set; }
    public Produkt Produkt { get; set; }
}
```

---

## 4. DbContext

### `DemoContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;

public class DemoContext : DbContext
{
    public DbSet<Kunde> Kunder { get; set; }
    public DbSet<Produkt> Produkter { get; set; }
    public DbSet<Ordre> Ordrer { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var conn = "server=localhost;port=4000;database=demo;user=root;password=demopass";
        optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kunde>().ToTable("Kunde");
        modelBuilder.Entity<Produkt>().ToTable("Produkt");
        modelBuilder.Entity<Ordre>().ToTable("Ordre");

        modelBuilder.Entity<Ordre>()
            .HasOne(o => o.Kunde)
            .WithMany()
            .HasForeignKey(o => o.KundeId);

        modelBuilder.Entity<Ordre>()
            .HasOne(o => o.Produkt)
            .WithMany()
            .HasForeignKey(o => o.ProduktId);
    }
}

```

---

## 5. Console Menu med LINQ og CRUD

### `Program.cs`
```csharp
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Denne manglede
using Microsoft.EntityFrameworkCore.Infrastructure; // Denne manglede

class Program
{
    static void Main()
    {
        using var db = new DemoContext();

        // Sikrer at EF kan forbinde (opretter ikke tabeller, da du har init.sql)
        Console.WriteLine("Forbinder til MySQL...");
        Console.WriteLine(db.Database.GetConnectionString()); // denne skulle ændres.
        Console.WriteLine("OK!\n");

        bool kører = true;
        while (kører)
        {
            Console.WriteLine("=== MENU ===");
            Console.WriteLine("1) Se alle kunder");
            Console.WriteLine("2) Se alle ordrer");
            Console.WriteLine("3) Se alle produkter");
            Console.WriteLine("4) Tilføj nyt produkt");
            Console.WriteLine("0) Afslut");
            Console.Write("Vælg et tal: ");

            var valg = Console.ReadLine();

            switch (valg)
            {
                case "1": VisKunder(db); break;
                case "2": VisOrdrer(db); break;
                case "3": VisProdukter(db); break;
                case "4": TilføjProdukt(db); break;
                case "0": kører = false; break;
                default: Console.WriteLine("❌ Ugyldigt valg.\n"); break;
            }
        }
    }

    static void VisKunder(DemoContext db)
    {
        Console.WriteLine("\n=== Alle kunder ===");
        var kunder = db.Kunder.ToList();
        foreach (var k in kunder)
            Console.WriteLine($"{k.Id}: {k.Navn} ({k.Alder} år)");
        Console.WriteLine();
    }

    static void VisOrdrer(DemoContext db)
    {
        Console.WriteLine("\n=== Ordrer med kunde og produkt ===");
        var ordrer = from o in db.Ordrer
                     join k in db.Kunder on o.KundeId equals k.Id
                     join p in db.Produkter on o.ProduktId equals p.Id
                     select new { Kunde = k.Navn, Produkt = p.Navn, o.Antal };

        foreach (var o in ordrer)
            Console.WriteLine($"{o.Kunde} køber {o.Antal} x {o.Produkt}");
        Console.WriteLine();
    }

    static void VisProdukter(DemoContext db)
    {
        Console.WriteLine("\n=== Produkter ===");
        var produkter = db.Produkter.ToList();
        foreach (var p in produkter)
            Console.WriteLine($"{p.Id}: {p.Navn} - {p.Pris} kr");
        Console.WriteLine();
    }

    static void TilføjProdukt(DemoContext db)
    {
        Console.WriteLine("\n=== Tilføj nyt produkt ===");
        Console.Write("Navn: ");
        string navn = Console.ReadLine() ?? "";

        Console.Write("Pris: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal pris))
        {
            var nytProdukt = new Produkt { Navn = navn, Pris = pris };
            db.Produkter.Add(nytProdukt);
            db.SaveChanges();
            Console.WriteLine($"✅ '{nytProdukt.Navn}' tilføjet (pris {nytProdukt.Pris} kr).\n");
        }
        else
        {
            Console.WriteLine("❌ Ugyldig pris.\n");
        }
    }
}

```

---

## 6. Kør øvelsen

1. Start databasen:
   ```bash
   cd db-demo
   docker compose up -d
   ```

2. Kør C# appen:
   ```bash
   cd ../LinqDemo
   dotnet run
   ```

---

## ✅ Output-eksempel

```
=== MENU ===
1) Se alle kunder
2) Se alle ordrer
3) Se alle produkter
4) Tilføj nyt produkt
0) Afslut
Vælg et tal: 3

=== Produkter ===
1: Laptop - 8999.95 kr
2: Mus - 199.95 kr
3: Skærm - 1599.00 kr
```

---

## 7. Udvidet Øvelse (selvstændigt arbejde)

Nu skal du selv udvide programmet med nye funktioner.  

👉 Opgaver:  
1. Tilføj en **menu-mulighed for at oprette kunder**.  
   - Brug `Console.ReadLine()` til at spørge om navn og alder.  
   - Gem den nye kunde i databasen med `db.Kunder.Add(...)` og `db.SaveChanges()`.  

2. Tilføj en **menu-mulighed for at slette produkter**.  
   - Spørg om et produkt-ID.  
   - Find produktet med `db.Produkter.FirstOrDefault(p => p.Id == id)`.  
   - Hvis det findes, slet det med `db.Produkter.Remove(...)` og gem ændringen.  

3. (Bonus) Tilføj en **menu-mulighed for at slette kunder**.  
   - Husk at kunder kan have ordrer → diskuter hvad der sker i databasen, hvis man sletter en kunde med ordrer.  
   - Kan I lave en løsning hvor ordrer også slettes automatisk (cascade delete)?  

---

## 🚀 Refleksionsspørgsmål
- Hvorfor er det en fordel at bruge LINQ fremfor rå SQL-forespørgsler?  
- Hvad sker der i databasen, når man kalder `db.SaveChanges()`?  
- Hvordan kan man undgå at slette en kunde, der har ordrer, uden at ødelægge databasen?  
- Hvordan kunne programmet udvides til at håndtere **brugervalg via menuer på en mere skalerbar måde** (fx `switch` → metoder → services)?  

---
