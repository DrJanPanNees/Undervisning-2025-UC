# 🌦️ Weather Demo with Nginx, MySQL and YARP

This project shows a simple setup with three components:  

- **Nginx (port 5001)** → serves a static HTML page  
- **MySQL (port 5002)** → stores weather data  
- **YARP Reverse Proxy (port 4000)** → exposes both Nginx and the weather API behind a single endpoint  

---

## 🚀 Project Structure

```
weather-demo/
├── docker-compose.yml
├── mysql-init/
│   └── init.sql
├── html/
│   └── index.html
└── YarpProxy/
    ├── Program.cs
    └── appsettings.json
```

---

## 1️⃣ Docker Compose

**docker-compose.yml**
```yaml
version: "3.9"

services:
  nginx:
    image: nginx:alpine
    container_name: nginx-weather
    ports:
      - "5001:80"
    volumes:
      - ./html:/usr/share/nginx/html:ro

  mysql:
    image: mysql:8.0
    container_name: mysql-weather
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: weatherdb
      MYSQL_USER: weatheruser
      MYSQL_PASSWORD: weatherpass
    ports:
      - "5002:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./mysql-init:/docker-entrypoint-initdb.d:ro

volumes:
  mysql_data:
```

---

## 2️⃣ MySQL Init Script

**mysql-init/init.sql**
```sql
CREATE TABLE weather (
  id INT AUTO_INCREMENT PRIMARY KEY,
  city VARCHAR(50),
  temperature DECIMAL(5,2),
  recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO weather (city, temperature) VALUES
('Vejle', 17.5),
('Herning', 16.0),
('Odense', 18.3);
```

---

## 3️⃣ HTML Page

**html/index.html**
```html
<!DOCTYPE html>
<html lang="da">
<head>
  <meta charset="UTF-8">
  <title>Vejrdata</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 2rem; }
    h1 { color: #333; }
    table { border-collapse: collapse; margin-top: 1rem; width: 60%; }
    th, td { border: 1px solid #ccc; padding: 0.5rem 1rem; text-align: left; }
    th { background-color: #f5f5f5; }
  </style>
</head>
<body>
  <h1>Vejrdata fra databasen</h1>
  <table id="weather-table">
    <thead>
      <tr>
        <th>By</th>
        <th>Temperatur (°C)</th>
        <th>Tidspunkt</th>
      </tr>
    </thead>
    <tbody></tbody>
  </table>

  <script>
    async function loadWeather() {
      try {
        const response = await fetch('/api/weather');
        const data = await response.json();

        const tbody = document.querySelector('#weather-table tbody');
        tbody.innerHTML = '';

        data.forEach(row => {
          const tr = document.createElement('tr');
          tr.innerHTML = `
            <td>${row.city}</td>
            <td>${row.temperature}</td>
            <td>${new Date(row.recordedAt).toLocaleString()}</td>
          `;
          tbody.appendChild(tr);
        });
      } catch (err) {
        console.error('Fejl ved hentning af vejrdata:', err);
      }
    }

    loadWeather();
  </script>
</body>
</html>
```

---

## 4️⃣ YARP Proxy

### Program.cs
```csharp
using MySql.Data.MySqlClient;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// API to fetch weather data from MySQL
app.MapGet("/api/weather", async () =>
{
    var weatherData = new List<object>();
    var connStr = "server=localhost;port=5002;database=weatherdb;user=weatheruser;password=weatherpass";

    using var conn = new MySqlConnection(connStr);
    await conn.OpenAsync();

    var cmd = new MySqlCommand("SELECT city, temperature, recorded_at FROM weather", conn);
    using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        weatherData.Add(new
        {
            City = reader.GetString(0),
            Temperature = reader.GetDecimal(1),
            RecordedAt = reader.GetDateTime(2)
        });
    }

    return Results.Json(weatherData);
});

// YARP routes
app.MapReverseProxy();

app.Run("http://localhost:4000");
```

---

### appsettings.json
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
      "siteRoute": {
        "ClusterId": "nginxCluster",
        "Match": {
          "Path": "/api/site/{**catch-all}"
        },
        "Transforms": [
          { "PathRemovePrefix": "/api/site" }
        ]
      }
    },
    "Clusters": {
      "nginxCluster": {
        "Destinations": {
          "nginxBackend": {
            "Address": "http://localhost:5001/"
          }
        }
      }
    }
  }
}
```

---

## 5️⃣ Test

- **Website via nginx directly**  
  [http://localhost:5001/index.html](http://localhost:5001/index.html)

- **Website via YARP**  
  [http://localhost:4000/api/site/index.html](http://localhost:4000/api/site/index.html)

- **Weather API via YARP (from MySQL)**  
  [http://localhost:4000/api/weather](http://localhost:4000/api/weather)

---

## 6️⃣ Architecture Diagram

```
[ Browser ]
     |
     v
http://localhost:4000
     |
     +--> /api/site  ----> Nginx (5001) ----> index.html
     |
     +--> /api/weather ---> C# API (4000) ---> MySQL (5002)
```

---

## ✅ How to Run

1. Clone repo  
2. Run `docker compose up -d` to start nginx + mysql  
3. Start YARP project with `dotnet run` inside `/YarpProxy`  
4. Open browser at [http://localhost:4000/api/site/index.html](http://localhost:4000/api/site/index.html)  

---
