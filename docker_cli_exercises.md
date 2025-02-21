# Docker CLI Cheatsheet - Øvelser

## Øvelse 1: Start en container og undersøg den
### Meta-data
- **Læringsmål**: Forstå, hvordan man starter en container fra et image og inspicerer den.
- **Relevans**: Grundlæggende for alt arbejde med Docker.
- **Sværhedsgrad**: Let

### Opgave
1. Træk et image af nginx
   ```bash
   docker pull nginx
   ```
2. Start en ny container fra nginx-image og kald den "web"
   ```bash
   docker run --name web -d nginx
   ```
3. Vis alle kørende containere
   ```bash
   docker ps
   ```
4. Få detaljer om containeren
   ```bash
   docker inspect web
   ```

---

## Øvelse 2: Port-forwarding og adgang til container
### Meta-data
- **Læringsmål**: Lære at mappe porte mellem host og container.
- **Relevans**: Vigtigt for at tilgå tjenester kørende i containere.
- **Sværhedsgrad**: Let

### Opgave
1. Start en ny nginx-container, hvor port 8080 på din host maps til port 80 i containeren:
   ```bash
   docker run -d -p 8080:80 --name webserver nginx
   ```
2. Bekræft, at containeren kører:
   ```bash
   docker ps
   ```
3. Åbn en browser og besøg `http://localhost:8080`.

---

## Øvelse 3: Kopiering af filer mellem host og container
### Meta-data
- **Læringsmål**: Lære hvordan man kopierer filer mellem en host og en container.
- **Relevans**: Nødvendigt for at flytte data mellem systemer.
- **Sværhedsgrad**: Middel

### Opgave
1. Opret en fil `index.html` på din host:
   ```bash
   echo "<h1>Hello Docker</h1>" > index.html
   ```
2. Kopier filen til nginx-containerens HTML-mappe:
   ```bash
   docker cp index.html webserver:/usr/share/nginx/html/index.html
   ```
3. Besøg `http://localhost:8080` i din browser og se den opdaterede side.

---

## Øvelse 4: Administrer containere (Stop, Start, Slet)
### Meta-data
- **Læringsmål**: Få kontrol over livscyklussen for containere.
- **Relevans**: Kritisk for systemadministration.
- **Sværhedsgrad**: Let

### Opgave
1. Stop containeren:
   ```bash
   docker stop webserver
   ```
2. Start den igen:
   ```bash
   docker start webserver
   ```
3. Slet containeren:
   ```bash
   docker rm -f webserver
   ```

---

## Øvelse 5: Byg dit eget image
### Meta-data
- **Læringsmål**: Lære at oprette et Docker-image fra en Dockerfile.
- **Relevans**: Grundlæggende for CI/CD og DevOps.
- **Sværhedsgrad**: Middel

### Opgave
1. Opret en ny mappe og naviger til den:
   ```bash
   mkdir myweb && cd myweb
   ```
2. Opret en `Dockerfile` med følgende indhold:
   ```dockerfile
   FROM nginx
   COPY index.html /usr/share/nginx/html/index.html
   ```
3. Byg et image fra Dockerfile:
   ```bash
   docker build -t mynginx .
   ```
4. Start en container fra det nye image:
   ```bash
   docker run -d -p 8080:80 --name myweb mynginx
   ```
5. Test i browseren `http://localhost:8080`.

---

## Øvelse 6: Ryd op i Docker-miljøet
### Meta-data
- **Læringsmål**: Lære at rydde op i unødvendige images og containere.
- **Relevans**: Hjælper med at spare diskplads og holde systemet ryddeligt.
- **Sværhedsgrad**: Let

### Opgave
1. Slet ubrugte containere:
   ```bash
   docker container prune
   ```
2. Slet unødvendige images:
   ```bash
   docker image prune -a
   ```
3. Vis hvilke images der stadig er tilbage:
   ```bash
   docker images
   ```
