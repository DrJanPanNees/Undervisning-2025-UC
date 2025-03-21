
# 🧪 Øvelse 1 – Opsæt Docker Compose med MySQL, Adminer og NGINX

## 🎯 Mål
Du skal lære at sætte en container-baseret infrastruktur op med Docker Compose. Dette bliver fundamentet for hele kundesystemet.

---

## 📁 Filstruktur

Start med at oprette denne struktur:

```
kundesystem/
├── docker-compose.yml
├── db/
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

I roden af projektet (mappen `kundesystem`), opret filen `docker-compose.yml`:

```yaml
version: '3.8'

services:
  db:
    image: mysql:8.0
    container_name: mysql_db
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: root            # root-brugerens adgangskode
      MYSQL_DATABASE: webshop              # initial database der oprettes
    ports:
      - "3306:3306"                         # eksponerer MySQL til værten (valgfrit)
    volumes:
      - ./db/init:/docker-entrypoint-initdb.d
      # mappen med SQL-scripts køres automatisk ved første start

  adminer:
    image: adminer
    container_name: adminer_ui
    restart: always
    ports:
      - "8080:8080"                         # Adminer tilgås via http://localhost:8080

  nginx:
    image: nginx:latest
    container_name: nginx_web
    ports:
      - "80:80"                             # HTTP-webserver tilgængelig via port 80
    volumes:
      - ./nginx/default.conf:/etc/nginx/conf.d/default.conf
      # NGINX config mountes ind – tom config til at starte med
```

💡 **Forklaring:**
- `services` definerer hver container (MySQL, Adminer, NGINX)
- `volumes` mount’er lokale filer ind i containeren
- `ports` sørger for adgang udefra (localhost)

---

### 🔹 Tilføj SQL-init script

Opret og rediger filen `kundesystem/db/init/init.sql`:

```sql
CREATE DATABASE IF NOT EXISTS webshop;

USE webshop;

CREATE TABLE IF NOT EXISTS customers (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100),
  email VARCHAR(100)
);
```

🧠 *Dette script kører automatisk, første gang databasen starter.*

---

### 🔹 Tilføj tom NGINX config

Opret og rediger filen `kundesystem/nginx/default.conf`:

```nginx
server {
    listen 80;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
    }
}
```

🧠 *Vi tilføjer HTML-filer og routing senere i forløbet.*

---

### 🔹 Start systemet

I roden af `kundesystem/`:

```bash
docker compose up --build
```

Tjek at det virker:
- Adminer: [http://localhost:8080](http://localhost:8080)
  - Server: `db`  
  - Bruger: `root`  
  - Adgangskode: `root`  
  - Database: `webshop`

- NGINX: [http://localhost](http://localhost) (viser standard "Welcome to nginx")

---

## ✅ Delmål checkliste

| Delmål                                      | Status  |
|--------------------------------------------|---------|
| Lav `docker-compose.yml`                   | ✅       |
| Initialiser database med SQL-script        | ✅       |
| Tilføj tom NGINX-konfiguration             | ✅       |
| Start system og bekræft funktionalitet     | ✅       |

---

## 🧩 Hvad skal der ske i næste øvelse?

Du skal nu oprette en C#-konsolapplikation, der kan:

- Tilføje kunder til databasen
- Hente og vise alle kunder
- Slette en kunde

Denne applikation skal du også dockerisere, så den kan tilgå MySQL-containeren fra Compose-netværket. Det bliver Øvelse 2.

