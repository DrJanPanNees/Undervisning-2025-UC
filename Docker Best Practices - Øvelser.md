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
## Mappestruktur

```
node-docker-app/
├── Dockerfile
├── package.json
└── server.js
```

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

Lidt ekstra hjælp: 
---

## package.json

```json
{
  "name": "node-docker-app",
  "version": "1.0.0",
  "description": "Simple Node.js app for Docker exercise",
  "main": "server.js",
  "scripts": {
    "start": "node server.js"
  },
  "dependencies": {
    "express": "^4.18.2"
  }
}
```

---

## server.js

```js
const express = require("express");
const app = express();
const port = 3000;

app.get("/", (req, res) => {
  res.send("Hello from Docker + Node.js!");
});

app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});
```

---
(Ekstra øvelse, ikke påkrævet) Prøv at oprette en .dockerignore med følgende indhold:
```
node_modules
npm-debug.log
```
---
HUSK at bygge til sidst:  docker build -t node-app . 
---

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

# Øvelse 3: Brug af `.dockerignore` for mindre build-context og image (IKKE TESTET endnu, prøv gerne men ikke et krav!)

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

# Øvelse 4: Multi-stage build for at optimere image-størrelse

## Beskrivelse
I denne øvelse bruger du multi-stage builds til at adskille installation af afhængigheder fra det endelige runtime-image.  
Formålet er at minimere størrelsen af det image, som du kører i produktion, og kun have det nødvendige med.

## Læringsmål
- Forstå hvordan multi-stage builds fungerer i Docker.  
- Lære at adskille build-processen (fx installation af pakker) fra runtime.  
- Kunne sammenligne image-størrelser mellem et simpelt build og et multi-stage build.  

---

## Multi-stage Dockerfile

Opret en fil **`Dockerfile`** med dette indhold:

```dockerfile
# Første stage: installer afhængigheder
FROM node:18-alpine AS builder
WORKDIR /app

# Kopiér package-filer og installer afhængigheder
COPY package.json package-lock.json* ./
RUN npm install

# Kopiér kildekode
COPY . .

# Andet stage: rent runtime-image
FROM node:18-alpine
WORKDIR /app

# Kopiér kun nødvendige ting fra builder
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/server.js ./server.js
COPY --from=builder /app/package.json ./package.json

# Start appen
CMD ["node", "server.js"]
```

---

## Opgaver

1. Byg Docker-billedet med multi-stage:  
   ```bash
   docker build -t caching-demo-multi .
   ```

2. Sammenlign med det image du lavede i Øvelse 2:  
   ```bash
   docker image ls | grep caching-demo
   ```

3. Kør containeren:  
   ```bash
   docker run -p 3000:3000 caching-demo-multi
   ```

   Tjek [http://localhost:3000](http://localhost:3000) – appen kører som før.

4. Diskutér:  
   - Hvorfor er runtime-imaget mindre, selvom funktionaliteten er den samme?  
   - Hvilke filer har vi undgået at tage med i det endelige image?  
   - Hvordan hjælper dette i en produktion, hvor man måske har mange containere?

---

## Diskussion
Multi-stage builds gør det muligt at holde runtime-images “rene” – kun det nødvendige til at køre applikationen kommer med.  
Det giver:  
- Mindre images → hurtigere deploy.  
- Mindre angrebsflade → bedre sikkerhed.  
- Tydelig adskillelse mellem build-miljø og runtime-miljø.  


---

# Øvelse 5: Kør ikke containere som root

## Beskrivelse
Som standard kører en container som `root`. Det er en sikkerhedsrisiko, fordi et brud i containeren kan give adgang som root på værtsmaskinen.  
I denne øvelse lærer du at oprette og bruge en ikke-root-bruger i din container.

## Læringsmål
- Forstå hvorfor det er en risiko at køre containere som root.  
- Oprette en bruger i Dockerfile.  
- Køre applikationen som denne bruger i stedet for root.  

---

## Dockerfile med ikke-root bruger

```dockerfile
FROM node:18-alpine
WORKDIR /app

# Opret en gruppe og en bruger
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Kopiér kildekoden ind i mappen og skift ejerskab
COPY . .
RUN chown -R appuser:appgroup /app

# Skift til ikke-root bruger
USER appuser

CMD ["node", "server.js"]
```

---

## Opgaver

1. Byg Docker-billedet:  
   ```bash
   docker build -t caching-demo-nonroot .
   ```

2. Kør containeren:  
   ```bash
   docker run -p 3000:3000 caching-demo-nonroot
   ```

3. Kontroller hvilken bruger der kører processen:  
   - Find container-id:  
     ```bash
     docker ps
     ```  
   - Tjek brugeren:  
     ```bash
     docker exec -it <container-id> whoami
     ```  
   - Tjek processen:  
     ```bash
     docker exec -it <container-id> ps aux
     ```

---

## Diskussion
- Hvorfor er det en risiko at køre som root i en container?  
- Hvordan hjælper det at køre som en dedikeret app-bruger?  
- Kan man forestille sig situationer, hvor root stadig er nødvendigt?  


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


# Scan image for sårbarheder 
docker scout cves nginx

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
