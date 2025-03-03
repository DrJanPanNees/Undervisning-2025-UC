# Docker Øvelser

## Øvelse 1: Kør din første container

**Metadata:**

- **Emne:** Grundlæggende Docker
- **Mål:** Introduktion til Docker-containere
- **Relevans:** Forståelse af container-konceptet

**Instruktioner:**

1. Installer Docker (hvis ikke allerede installeret).
2. Kør følgende kommando for at starte en Ubuntu-container:
   ```bash
   docker run -it ubuntu /bin/bash
   ```
3. Kør `ls` og `pwd` inde i containeren for at inspicere filsystemet.
4. Afslut containeren ved at skrive `exit`.

---

## Øvelse 2: List og administrer containere

**Metadata:**

- **Emne:** Container administration
- **Mål:** Lære at liste, stoppe og slette containere
- **Relevans:** Effektiv administration af Docker-miljøet

**Instruktioner:**

1. Kør en ny Ubuntu-container:
   ```bash
   docker run -d --name test-container ubuntu sleep 600
   ```
2. List de kørende containere:
   ```bash
   docker ps
   ```
3. List alle containere, inklusive stoppede:
   ```bash
   docker ps -a
   ```
4. Stop containeren:
   ```bash
   docker stop test-container
   ```
5. Slet containeren:
   ```bash
   docker rm test-container
   ```

---

## Øvelse 3: Byg en Docker-image

**Metadata:**

- **Emne:** Docker Images
- **Mål:** Introduktion til at bygge Docker-images
- **Relevans:** Lær at containerisere applikationer


# Docker Øvelse: Simpel Node.js Webserver

## **Formål**
Denne øvelse viser, hvordan du kører en simpel Node.js-webserver i en Docker-container **uden at installere Node.js** på din host-computer.

## **Forudsætninger**
- **Kun Docker!** Ingen npm eller Node.js kræves på hosten.

---

## **1. Opret en ny projektmappe**
Åbn terminalen og kør:
```sh
mkdir simple-node-app && cd simple-node-app
```

---

## **2. Opret `server.js`**
Kør:
```sh
touch server.js
```
Eller opret filen manuelt og indsæt dette:

```javascript
const http = require('http');

const server = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello from Docker!\n');
});

const PORT = 3000;
server.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});
```

---

## **3. Opret en `Dockerfile`**
Kør:
```sh
touch Dockerfile
```
Indsæt følgende i `Dockerfile`:

```dockerfile
# Brug en letvægts Node.js-image
FROM node:18-alpine

# Sæt arbejdsmappe
WORKDIR /app

# Kopiér server.js til containeren
COPY server.js .

# Start serveren
CMD ["node", "server.js"]
```

---

## **4. Byg og kør containeren**
Byg containeren:
```sh
docker build -t simple-node-app .
```

Kør containeren:
```sh
docker run -p 3000:3000 simple-node-app
```

---

## **5. Test i browseren**
Åbn din browser og gå til:
```
http://localhost:3000
```
Du bør se:
```
Hello from Docker!
```

---

## **Hvorfor denne version?**
✅ **Ingen afhængigheder på hosten** – Alt kører i Docker.  
✅ **Ingen database eller eksterne services** – Kun en simpel webserver.  
✅ **Let at starte og stoppe** – Alt kan bygges og køres med Docker alene.

---

## **6. Stop og ryd op**
For at stoppe containeren:
```sh
CTRL + C
```

For at slette containeren:
```sh
docker rm $(docker ps -a -q) -f
```
For at slette Docker-imagen:
```sh
docker rmi simple-node-app -f
```

God fornøjelse med Docker! 🚀

---

## Øvelse 4: Docker Compose til multi-container applikationer

**Metadata:**

- **Emne:** Docker Compose
- **Mål:** Introduktion til Docker Compose
- **Relevans:** Håndtering af komplekse applikationer

**Instruktioner:**

1. Opret en `docker-compose.yaml` fil:
   ```yaml
   version: '3'
   services:
     web:
       image: nginx
       ports:
         - "8080:80"
   ```
2. Start tjenesten:
   ```bash
   docker compose up -d
   ```
3. Bekræft, at tjenesten kører:
   ```bash
   docker ps
   ```
4. Stop tjenesten:
   ```bash
   docker compose down
   ```

---

## Øvelse 5: Brug af Docker Volumes

**Metadata:**

- **Emne:** Datavedholdenhed
- **Mål:** Lære at bruge persistente volumener
- **Relevans:** Gemme data på tværs af container-genstart

**Instruktioner:**

1. Opret et volumen:
   ```bash
   docker volume create my-volume
   ```
2. Start en container med volumen:
   ```bash
   docker run -d --name vol-container -v my-volume:/data alpine sleep 600
   ```
3. Bekræft, at volumen eksisterer:
   ```bash
   docker volume ls
   ```
4. Slet containeren, men behold volumen:
   ```bash
   docker rm -f vol-container
   ```
5. Slet volumen:
   ```bash
   docker volume rm my-volume
   ```

---

## Øvelse 6: Udforskning af Docker-funktionalitet

**Metadata:**

- **Emne:** Selvstændig læring
- **Mål:** Styrke evnen til at finde og anvende Docker-koncepter
- **Relevans:** Forbedrer evnen til at opsøge viden og arbejde selvstændigt med teknologien

**Instruktioner:**

1. Find og opsæt en simpel webapplikation ved hjælp af Docker.
2. Beskriv hvilke kommandoer du brugte, og hvorfor.
3. Overvej brug af Docker-netværk, volumes eller multi-container setups (valgfrit).
4. Dokumentér dine fund i en Markdown-fil.
5. Diskuter dine valg med en medstuderende eller underviseren.

