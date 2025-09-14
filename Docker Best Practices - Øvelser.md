# Docker Best Practices - Øvelser

## Metadata
- **Emne:** Best Practices for Docker
- **Niveau:** Mellem til avanceret
- **Forudsætninger:** Grundlæggende kendskab til Docker, herunder Dockerfiles og containerhåndtering
- **Varighed:** 1-2 timer afhængigt af dybden
- **Læringsmål:**
  - Forstå og implementere Docker best practices
  - Optimere Docker-images for ydeevne og sikkerhed
  - Anvende caching og multi-stage builds
  - Skabe sikre Docker-miljøer

---

## Øvelse 1: Brug officielle og versionerede billeder
**Beskrivelse:**
Denne øvelse fokuserer på at vælge officielle og verificerede Docker-images samt at fastlåse versioner for at sikre stabilitet.

**Opgaver:**
1. Find et officielt Docker-image til **Node.js** på Docker Hub.
2. Skriv en Dockerfile, der bruger det officielle **Node.js** image.
3. Brug en specifik version af Node.js i stedet for `latest`.
4. Byg og kør containeren.

**Eksempel på Dockerfile:**
```dockerfile
# Dårlig praksis
# FROM node:latest

# Bedre praksis
FROM node:18-alpine

WORKDIR /app
COPY . .
RUN npm install
CMD ["node", "server.js"]
```

**Diskussion:** Hvorfor er `latest` en dårlig praksis? Hvordan påvirker versionering stabiliteten?

---

# Øvelse 2: Optimering af caching i Dockerfile

## Beskrivelse
I denne øvelse lærer du, hvordan du kan strukturere din Dockerfile, så du udnytter caching og minimerer build-tiden.  

## Læringsmål
- Forstå hvordan Docker cacher lag under build.  
- Oplev forskellen på at ændre afhængigheder vs. at ændre kode.  
- Kunne forklare hvorfor rækkefølgen i en Dockerfile betyder noget for build-tid.  

---

## Startkode

Opret en ny mappe og læg disse filer i den:

**`package.json`**
```json
{
  "name": "caching-demo",
  "version": "1.0.0",
  "description": "Demo app til caching i Docker",
  "main": "server.js",
  "scripts": {
    "start": "node server.js"
  },
  "dependencies": {
    "express": "^4.18.2"
  }
}
```

**`server.js`**
```js
const express = require("express");
const app = express();
const port = 3000;

app.get("/", (req, res) => {
  res.send("Hej fra Docker caching demo!");
});

app.listen(port, () => {
  console.log(`Server kører på http://localhost:${port}`);
});
```

---

## Dockerfile

**`Dockerfile`**
```dockerfile
FROM node:18-alpine
WORKDIR /app

# Kopiér kun package-filer først for at udnytte caching
COPY package.json package-lock.json* ./
RUN npm install

# Kopiér resten af projektet
COPY . .

CMD ["node", "server.js"]
```

> Bemærk: `package-lock.json*` gør, at den ikke fejler, hvis der ikke findes en lock-fil.

---

## Opgaver

1. Byg Docker-billedet første gang:  
   ```bash
   docker build -t caching-demo .
   ```
2. Kør containeren:  
   ```bash
   docker run -p 3000:3000 caching-demo
   ```
   Tjek i browseren: [http://localhost:3000](http://localhost:3000)

3. Lav en ændring i **server.js** (fx ændr teksten i `res.send`).  
   - Byg igen med `docker build ...`  
   - Undersøg hvilke trin bliver genbrugt fra cache?  

4. Lav en ændring i **package.json** (fx tilføj en ny dependency med `npm install nodemon --save`).  
   - Byg igen.  
   - Hvad sker der nu? Hvorfor bliver `npm install` kørt igen?  

---

## Diskussion
- Hvorfor bliver `npm install` kun kørt, når `package.json` ændres?  
- Hvordan hjælper det med at spare tid i store projekter?  
- Hvad ville der ske, hvis vi havde kopieret hele projektet *før* `npm install`?  


---

# Øvelse 3: Brug af `.dockerignore` for mindre build-context og image

**Formål:** Undgå at sende/indbygge unødvendige filer i dit Docker-image. Det giver hurtigere builds og ofte mindre images.

> **Note:** `.dockerignore` reducerer først og fremmest *build-contexten* (det der sendes til Docker ved `docker build`). Image-størrelsen bliver også mindre, hvis de ignorerede filer ellers ville være kopieret ind via `COPY . .`.

---

## Forudsætning
Brug mappen fra **Øvelse 2** (den lille Node/Express-app med `Dockerfile` der har `COPY . .` til sidst).

---

## Trin

1. **(Kun hvis du ikke har et Git-repo og nogle filer at ignorere)**  
   ```bash
   git init
   git add .
   git commit -m "init"
   mkdir -p logs && echo "lorem ipsum" > logs/app.log
   # Valgfrit: skab lidt volumen for at se forskellen tydeligt
   dd if=/dev/zero of=logs/big.log bs=1M count=20 2>/dev/null
   # Hvis du vil vise node_modules-effekten:
   npm install
   ```
   > Du får nu en `.git/` mappe, en `logs/` mappe og evt. `node_modules/`.

2. **Byg uden `.dockerignore` (baseline):**
   ```bash
   docker build --no-cache -t caching-demo:no-ignore .
   ```

3. **Opret `.dockerignore` i projektroden:**
   ```gitignore
   node_modules/
   logs/
   .git/
   .DS_Store
   .env
   ```
   > `node_modules/`: undgå at kopiere hostens moduler ind i imaget  
   > `logs/`: runtime-filer hører ikke til i build  
   > `.git/`: stort og ændrer sig hele tiden – ødelægger caching og fylder i imaget  
   > `.env`: hemmeligheder skal ikke ind i imaget  
   > `.DS_Store`: macOS-støj

4. **Byg med `.dockerignore`:**
   ```bash
   docker build --no-cache -t caching-demo:ignore .
   ```

5. **Sammenlign størrelser og build-tid:**
   ```bash
   docker image ls | grep caching-demo
   ```
   Du bør se, at `:ignore` typisk er mindre end `:no-ignore` (især hvis `.git/` og `node_modules/` ellers var blevet kopieret). Build-tiden er også hurtigere, fordi der sendes mindre data til Docker.

6. **(Valgfrit) Verificér at ignorerede filer ikke ender i imaget:**
   ```bash
   docker run --rm -it caching-demo:ignore sh -c "ls -la | head -n 50 && echo '---' && [ -d .git ] && echo '.git findes' || echo '.git findes ikke'"
   ```

---

## Refleksion
- Hvilke filer gav størst forskel – `.git`, `node_modules` eller `logs`?  
- Hvordan påvirker `.git/` caching, hvis du committer ofte?  
- Hvorfor er det en dårlig idé at bake `.env` (hemmeligheder) ind i et image?

---

## Bonus
- Prøv at fjerne `logs/` fra `.dockerignore` og rebuild – blev imaget større?  
- Tilføj en fil i repoet og commit igen. Med `.git/` i `.dockerignore` bør build-cachen være mere stabil.


---

## Øvelse 4: Multi-stage build for at optimere image-størrelse
**Beskrivelse:**
Brug multi-stage builds til at adskille build-processen fra det endelige image og reducere dets størrelse.

**Opgaver:**
1. Skriv en multi-stage Dockerfile til en Node.js-applikation.
2. Brug én fase til at bygge kildekoden og en anden til at køre applikationen.
3. Byg og kør containeren. Sammenlign billedstørrelsen før og efter optimering.

**Eksempel på multi-stage build:**
```dockerfile
# Build stage
FROM node:18-alpine AS builder
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm install
COPY . .
RUN npm run build

# Runtime stage
FROM node:18-alpine
WORKDIR /app
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules
CMD ["node", "dist/server.js"]
```

**Diskussion:** Hvordan reducerer multi-stage builds billedstørrelsen?

---

## Øvelse 5: Kør ikke containere som root
**Beskrivelse:**
Forbedr sikkerheden ved at oprette og bruge en ikke-root-bruger i din container.

**Opgaver:**
1. Opret en bruger i din Dockerfile.
2. Skift til denne bruger, før applikationen startes.
3. Byg og kør containeren. Kontroller hvilken bruger, der kører processen.

**Eksempel på Dockerfile med ikke-root bruger:**
```dockerfile
FROM node:18-alpine
WORKDIR /app
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser
COPY . .
CMD ["node", "server.js"]
```

**Diskussion:** Hvorfor er det en sikkerhedsrisiko at køre som root i en container?

---

## Øvelse 6: Scanning af Docker-images for sårbarheder
**Beskrivelse:**
Brug Docker Scan til at identificere sårbarheder i dit image.

**Opgaver:**
1. Byg et Docker-image af din applikation.
2. Log ind på Docker Hub (`docker login`).
3. Scan dit image for sårbarheder med `docker scan`.
4. Analyser resultatet og find måder at forbedre sikkerheden.

**Kommandoer:**
```sh
# Byg Docker image
docker build -t myapp .

# Scan image for sårbarheder
docker scout myapp
```

**Diskussion:** Hvordan kan du løse de identificerede sårbarheder?

---

## Afsluttende Refleksion
Efter at have gennemført øvelserne, diskuter:
1. Hvilke ændringer gjorde den største forskel i build-tid og billedstørrelse?
2. Hvordan kan disse best practices anvendes i et større DevOps-miljø?
3. Hvilke værktøjer kan hjælpe med automatisering af sikkerhed og optimering af Docker-billeder?

---

**Ekstra Ressourcer:**
- [Docker Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [Docker Security Guide](https://docs.docker.com/engine/security/)

God fornøjelse med øvelserne! 🚀
