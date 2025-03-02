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

**Instruktioner:**

1. Opret en ny mappe og skab en `Dockerfile`:
   ```dockerfile
   FROM alpine
   RUN apk add --update nodejs npm
   COPY . /src
   WORKDIR /src
   RUN npm install
   CMD ["node", "app.js"]
   ```
2. Byg Docker-imagen:
   ```bash
   docker build -t my-node-app .
   ```
3. Bekræft, at imagen er oprettet:
   ```bash
   docker images
   ```

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

