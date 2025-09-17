# Visualisering: C# → Docker → MySQL

## ASCII Diagram

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

## Forklaring

- Din **C# applikation** kører på hostmaskinen.  
- I connection string vælger du fx `Port=3307`.  
- Docker **mapper** denne port til containerens port **3306** (som MySQL bruger internt).  
- Trafikken går:  
  `localhost:3307  →  docker port mapping  →  mysql-demo:3306`  

---

## Eksempel på docker-compose.yml

```yaml
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
```

---

## Eksempel på C# connection string

```csharp
var connString = "Server=localhost;Port=3307;Database=demo;Uid=demo;Pwd=demopass;";
```

---

## Relevans

- **Forståelse af netværk:** Viser hvordan applikationer taler med containere via porte.  
- **Praktisk brug:** Bruges direkte i semesterprojektet, hvor C# skal kommunikere med MySQL i Docker.  
- **Generalisering:** Samme princip gælder for andre services (fx API-containere, Redis, Kafka osv.).  
