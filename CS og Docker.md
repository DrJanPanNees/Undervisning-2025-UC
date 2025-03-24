# **Øvelse: Kør en C#-applikation i en Docker-container med MySQL og YAML**

## **Mål**
I denne øvelse vil vi:
1. Udvikle en simpel C#-konsolapplikation.
2. Skrive en `Dockerfile` til at bygge og køre applikationen i en container.
3. Oprette en `docker-compose.yaml`-fil til at starte applikationen sammen med en MySQL-database.
4. Skrive kode til at gemme og hente data fra databasen.

---

## **Trin 1: Opret en simpel C#-konsolapplikation**
Start med at oprette en ny C#-konsolapplikation:

```sh
mkdir CSharpDockerApp
cd CSharpDockerApp
dotnet new console -o App
cd App
```

### **Installer MySQL Connector**
For at kunne kommunikere med MySQL skal vi installere `MySql.Data` pakken:

```sh
dotnet add package MySql.Data
```

Erstat indholdet af `Program.cs` med følgende kode:

```csharp
using System;
using MySql.Data.MySqlClient;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Server=mysql;Database=testdb;User=root;Password=root;";
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        
        string createTableQuery = "CREATE TABLE IF NOT EXISTS messages (id INT AUTO_INCREMENT PRIMARY KEY, text VARCHAR(255));";
        using var createTableCmd = new MySqlCommand(createTableQuery, connection);
        createTableCmd.ExecuteNonQuery();
        
        Console.Write("Indtast en besked: ");
        string message = Console.ReadLine();
        
        string insertQuery = "INSERT INTO messages (text) VALUES (@message);";
        using var insertCmd = new MySqlCommand(insertQuery, connection);
        insertCmd.Parameters.AddWithValue("@message", message);
        insertCmd.ExecuteNonQuery();
        
        Console.WriteLine("Beskeden er gemt i databasen.");
        
        string selectQuery = "SELECT * FROM messages;";
        using var selectCmd = new MySqlCommand(selectQuery, connection);
        using var reader = selectCmd.ExecuteReader();
        
        Console.WriteLine("\nGemte beskeder:");
        while (reader.Read())
        {
            Console.WriteLine($"{reader["id"]}: {reader["text"]}");
        }
    }
}
```

Koden:
- Opretter en databaseforbindelse.
- Sikrer, at tabellen `messages` eksisterer.
- Indsætter en brugerindtastet besked i databasen.
- Henter og viser alle gemte beskeder.

---

## **Trin 2: Opret en Dockerfile**
Opret en fil `Dockerfile` i `App`-mappen med følgende indhold:

```dockerfile
# Byg C#-applikationen
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .
CMD ["dotnet", "App.dll"]
```

---

## **Trin 3: Opret en `docker-compose.yaml`**
Opret en `docker-compose.yaml`-fil i roden af projektet:

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: mysql_container
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: testdb
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    restart: always

  csharp-app:
    build: ./App
    depends_on:
      - mysql
    environment:
      - DB_HOST=mysql
      - DB_NAME=testdb
      - DB_USER=root
      - DB_PASSWORD=root
    stdin_open: true
    tty: true

volumes:
  mysql_data:
```

**Forklaring:**
- **MySQL-container:** Opretter en database med navnet `testdb`, brugeren `root`, og adgangskoden `root`.
- **C#-applikation:** Starter efter databasen og bruger miljøvariabler til databaseforbindelsen.
- **Volumes:** Bevarer database-data mellem container-genstarter.

---

## **Trin 4: Byg og kør containeren**

Byg og start hele systemet med:

```sh
docker compose run --rm app
```

Herefter kan du indtaste en besked, som gemmes i MySQL-databasen, og se tidligere gemte beskeder.

---

## **Bonus-opgaver**
1. **Udvid applikationen:** Lav en menu, hvor brugeren kan slette eller opdatere beskeder.
2. **Tilføj en webservice:** Lav en simpel API med ASP.NET Core til at hente og indsende beskeder.
3. **Tilføj en frontend:** Opret en React eller Vue.js frontend, der viser data fra databasen.

---

Denne øvelse giver de studerende praktisk erfaring med **C#, Docker, MySQL og YAML**, og de får en forståelse for, hvordan containeriserede applikationer kommunikerer med databaser.

