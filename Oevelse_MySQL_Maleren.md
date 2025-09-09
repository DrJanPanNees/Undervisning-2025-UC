
# Øvelse: Installation og brug af MySQL i Linux Mint

**Formål:**  
At kunne installere MySQL, logge ind som root, og arbejde med databaser og tabeller relateret til semesterprojektet *Maleren A/S*.

---

## 🔧 Del 1: Installer MySQL på Linux Mint

1. Åbn Terminal  
2. Kør følgende kommandoer:

```
sudo apt update
sudo apt install mysql-server
sudo systemctl start mysql
sudo systemctl status mysql
```

3. Du skal se en linje som:
```
Active: active (running)
```

4. Log ind i MySQL som root:
```
sudo mysql
```

Du er nu i MySQLs kommandolinje.

---

## 🛠️ Del 2: Opret database og vælg den

1. Opret en ny database:
```
CREATE DATABASE maleren;
```

2. Vælg databasen:
```
USE maleren;
```

---

## 📦 Del 3: Opret tabeller inspireret af semesterprojektet

### Privatkunder:
```sql
CREATE TABLE privatkunde (
  id INT AUTO_INCREMENT PRIMARY KEY,
  navn VARCHAR(100),
  email VARCHAR(100),
  telefon VARCHAR(20),
  oprettet_dato DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Virksomhedskunder (B2B):
```sql
CREATE TABLE virksomhedskunde (
  id INT AUTO_INCREMENT PRIMARY KEY,
  virksomhedsnavn VARCHAR(100),
  cvr VARCHAR(10),
  adresse VARCHAR(255),
  email VARCHAR(100),
  kontaktperson VARCHAR(100),
  mobil VARCHAR(20),
  kreditgrænse DECIMAL(10,2),
  godkendt BOOLEAN DEFAULT FALSE,
  oprettet_dato DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Produkter:
```sql
CREATE TABLE produkt (
  id INT AUTO_INCREMENT PRIMARY KEY,
  navn VARCHAR(100),
  beskrivelse TEXT,
  pris DECIMAL(10,2),
  kategori VARCHAR(100),
  lagerantal INT
);
```

### Ordrer:
```sql
CREATE TABLE ordre (
  id INT AUTO_INCREMENT PRIMARY KEY,
  kunde_type ENUM('privat', 'b2b'),
  kunde_id INT,
  ordre_dato DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Ordrelinjer:
```sql
CREATE TABLE ordrelinje (
  id INT AUTO_INCREMENT PRIMARY KEY,
  ordre_id INT,
  produkt_id INT,
  antal INT,
  note TEXT,
  FOREIGN KEY (ordre_id) REFERENCES ordre(id),
  FOREIGN KEY (produkt_id) REFERENCES produkt(id)
);
```

---

## 🧠 Øvelsesspørgsmål

- Hvad er forskellen på en privatkunde og en virksomhedskunde i systemet?
- Hvordan gemmes relationen mellem en ordre og dens produkter?
- Hvordan kunne man sikre, at en B2B-kunde kun kan afgive ordrer når de er godkendt?
- Hvordan ville du tilføje rabat-logik i databasen?

---

## 📌 Bonusopgave

Udvid databasen med en tabel til rabatter baseret på kundetype eller produktkategori.

Eksempel:
```sql
CREATE TABLE rabat (
  id INT AUTO_INCREMENT PRIMARY KEY,
  kunde_type ENUM('privat', 'b2b'),
  kategori VARCHAR(100),
  rabat_procent DECIMAL(5,2)
);
```
