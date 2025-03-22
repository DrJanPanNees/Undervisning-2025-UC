# 🧪 Øvelse 1 – Opsæt Docker Compose med MySQL, Adminer og NGINX

## 🌟 Mål
Du skal lære at sætte en container-baseret infrastruktur op med Docker Compose. Dette bliver fundamentet for hele kundesystemet.

---

## 📁 Filstruktur

Start med at oprette denne struktur:

```
kundesystem/
├── docker-compose.yml
├── customer-db/
│   └── init/
│       └── init.sql
├── order-db/
│   └── init/
│       └── init.sql
├── product-db/
│   └── init/
│       └── init.sql
├── nginx/
│   └── default.conf
```

> Du opretter mapperne og filerne én for én i øvelsen herunder.

---

## 🔧 Trin-for-trin guide

---

### 🔹 Opret `docker-compose.yml`

```yaml
version: '3.8'

services:
  customerdb:
    image: mysql:8.0
    container_name: customer_db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: customer
    ports:
      - "3307:3306"
    volumes:
      - ./customer-db/init:/docker-entrypoint-initdb.d

  orderdb:
    image: mysql:8.0
    container_name: order_db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: orders
    ports:
      - "3308:3306"
    volumes:
      - ./order-db/init:/docker-entrypoint-initdb.d

  productdb:
    image: mysql:8.0
    container_name: product_db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: product
    ports:
      - "3309:3306"
    volumes:
      - ./product-db/init:/docker-entrypoint-initdb.d

  adminer:
    image: adminer
    container_name: adminer_ui
    restart: always
    ports:
      - "8080:8080"

  nginx:
    image: nginx:latest
    container_name: nginx_web
    ports:
      - "80:80"
    volumes:
      - ./nginx/default.conf:/etc/nginx/conf.d/default.conf
```

---

### 🔹 Opret init.sql til de tre databaser

**customer-db/init/init.sql**
```sql
CREATE TABLE IF NOT EXISTS customers (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100),
  email VARCHAR(100)
);
```

**order-db/init/init.sql**
```sql
CREATE TABLE IF NOT EXISTS orders (
  id INT AUTO_INCREMENT PRIMARY KEY,
  customer_id INT,
  product_id INT,
  quantity INT
);
```

**product-db/init/init.sql**
```sql
CREATE TABLE IF NOT EXISTS products (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100),
  price DECIMAL(10,2)
);
```

---

## 🧩 Øvelse 2 – CLI app: Vis kunder, produkter og ordrer

Du skal oprette en C#-konsolapplikation, der viser data fra alle tre databaser. Den skal vise kunder, produkter og ordrer i én samlet oversigt.

### 🔹 Trin 1 – Opret projekt og installer NuGet-pakke

```bash
dotnet new console -n MicroserviceViewer
cd MicroserviceViewer
dotnet add package MySql.Data
```

### 🔹 Trin 2 – Tilføj forbindelsesoplysninger og kode

```csharp
using MySql.Data.MySqlClient;

string customerConnStr = "server=localhost;port=3307;user=root;password=root;database=customer";
string orderConnStr = "server=localhost;port=3308;user=root;password=root;database=orders";
string productConnStr = "server=localhost;port=3309;user=root;password=root;database=product";

Console.WriteLine("\n--- Kunder ---");
using (var conn = new MySqlConnection(customerConnStr))
{
    conn.Open();
    var cmd = new MySqlCommand("SELECT * FROM customers", conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader["id"]}: {reader["name"]} – {reader["email"]}");
}

Console.WriteLine("\n--- Produkter ---");
using (var conn = new MySqlConnection(productConnStr))
{
    conn.Open();
    var cmd = new MySqlCommand("SELECT * FROM products", conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader["id"]}: {reader["name"]} – {reader["price"]} DKK");
}

Console.WriteLine("\n--- Ordrer ---");
using (var conn = new MySqlConnection(orderConnStr))
{
    conn.Open();
    var cmd = new MySqlCommand("SELECT * FROM orders", conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"Ordre ID: {reader["id"]} – Kunde: {reader["customer_id"]}, Produkt: {reader["product_id"]}, Antal: {reader["quantity"]}");
}
```

### 🔹 Trin 3 – Kør programmet

```bash
dotnet run
```

Du bør nu få en oversigt over alle tre mikroservices data.

---

## 🗑️ Øvelse 3 – Udvid CLI med slet funktioner

I denne øvelse skal du udvide C#-applikationen, så du kan slette:
- En kunde
- En ordre
- Et produkt

Brug samme struktur som tidligere, og tilføj menupunkter og slet-funktioner med SQL `DELETE`-kommandoer baseret på ID.

Eksempel:
```csharp
Console.WriteLine("Indtast ID på produkt der skal slettes:");
int id = int.Parse(Console.ReadLine());
var deleteCmd = new MySqlCommand("DELETE FROM products WHERE id = @id", conn);
deleteCmd.Parameters.AddWithValue("@id", id);
deleteCmd.ExecuteNonQuery();
Console.WriteLine("Produkt slettet.");
```

---

I næste øvelse kan du begynde at oprette ordrer og kombinere kundedata og produktdata til en egentlig webshop-struktur.

