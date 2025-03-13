# OWASP & Web Security

## Introduktion

### OWASP & Web Security

- Hvad er OWASP?
- Hvorfor er web security vigtig?
- Introduktion til OWASP Juice Shop

## Hvad er OWASP?

- Open Web Application Security Project (OWASP)
- Global non-profit organisation
- Fokuserer på at forbedre web-applikationers sikkerhed
- Udvikler gratis værktøjer, dokumentation og standarder

## OWASP Top 10

- De mest kritiske sikkerhedssårbarheder
- Opdateres regelmæssigt
- Hjælper udviklere og sikkerhedsfolk med at beskytte webapplikationer

## OWASP Top 10 - 2021 Version

1. Broken Access Control
2. Cryptographic Failures
3. Injection
4. Insecure Design
5. Security Misconfiguration
6. Vulnerable and Outdated Components
7. Identification and Authentication Failures
8. Software and Data Integrity Failures
9. Security Logging and Monitoring Failures
10. Server-Side Request Forgery (SSRF)

## Hvordan OWASP Top 10 påvirker udviklere

- Hjælper udviklere med at forstå risici
- Guide til sikrere kodning
- Hjælper organisationer med compliance og best practices

## Introduktion til OWASP Juice Shop

- Intentionally insecure webapplikation
- Bruges til at træne og teste sikkerhedsfærdigheder
- Open-source og baseret på moderne webteknologier

## Hvorfor bruge Juice Shop?

- Hands-on træning i web security
- Simulerer virkelige angrebsscenarier
- Perfekt til undervisning og selvstudie

## Hands-on – Opsætning af OWASP Juice Shop

### Krav

- Docker skal være installeret på dit system

### Trin-for-trin guide: Opsætning af Juice Shop i en Docker-container

1. **Installer Docker (hvis du ikke allerede har det)**
   - Gå til [Docker's hjemmeside](https://www.docker.com/get-started)
   - Download og installer Docker til dit operativsystem

2. **Åbn en terminal**
   - På Windows: Åbn `PowerShell` eller `Kommandoprompt`
   - På macOS/Linux: Åbn `Terminal`

3. **Download og start Juice Shop-containeren**
   - Kør følgende kommando:
     ```sh
     docker run -d -p 3000:3000 bkimminich/juice-shop
     ```
   - Denne kommando gør følgende:
     - `-d` kører containeren i baggrunden
     - `-p 3000:3000` eksponerer port 3000 fra containeren til din maskine
     - `bkimminich/juice-shop` er det officielle Docker-image

4. **Bekræft at containeren kører**
   - Kør følgende kommando:
     ```sh
     docker ps
     ```
   - Hvis Juice Shop kører, vil den være listet i outputtet

5. **Åbn Juice Shop i en browser**
   - Gå til `http://localhost:3000`
   - Du burde nu se Juice Shop's hjemmeside

6. **Fejlfinding**
   - Hvis Juice Shop ikke starter korrekt:
     - Tjek logs med: `docker logs <container-id>`
     - Genstart containeren: `docker restart <container-id>`
     - Stop og slet containeren: `docker rm -f <container-id>` og start igen

## Hands-on – Øvelser

### Identificer og udnyt sikkerhedshuller i Juice Shop

Her er nogle praktiske øvelser, hvor du kan teste dine sikkerhedsevner:

1. **SQL Injection**
   - Gå til login-siden
   - Indtast følgende i brugernavnsfeltet:
     ```
     ' OR 1=1 --
     ```
   - Hvis du nu bliver logget ind uden korrekt adgangskode, har du fundet en SQL Injection-sårbarhed

2. **Cross-Site Scripting (XSS)**
   - Find et inputfelt, f.eks. søgefunktionen
   - Indsæt følgende kode:
     ```html
     <script>alert('XSS')</script>
     ```
   - Hvis en pop-up vises, har du fundet en XSS-sårbarhed

3. **Broken Authentication**
   - Opret en ny bruger, og test om du kan logge ind uden korrekt adgangskode

4. **Directory Traversal**
   - Prøv at tilgå skjulte filer ved at ændre URL’en til:
     ```
     http://localhost:3000/../../../../etc/passwd
     ```
   - Hvis du får adgang til systemfiler, er der en Directory Traversal-sårbarhed

5. **Security Misconfiguration**
   - Undersøg udviklerkonsollen (`F12` i browseren) for at finde eksponerede API-kald

6. **Broken Access Control**
   - Find måder at omgå login-siden og tilgå admin-paneler

7. **CSRF (Cross-Site Request Forgery)**
   - Prøv at udnytte sessionshåndtering ved at sende uautoriserede requests

8. **Insecure Direct Object References (IDOR)**
   - Manipuler ID’er i URL’er for at få adgang til andres data

9. **Using Components with Known Vulnerabilities**
   - Undersøg brugte bibliotekers sikkerhed via udviklerværktøjer

10. **Logging og Overvågning**
   - Test om angreb bliver logget korrekt i Juice Shop

## Konklusion

- OWASP er afgørende for web security
- OWASP Top 10 er et must-know for udviklere
- Juice Shop giver praktisk erfaring med web security

## Spørgsmål?

- Q&A session

