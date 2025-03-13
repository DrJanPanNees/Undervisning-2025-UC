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

Her er nogle praktiske øvelser, hvor du kan teste dine sikkerhedsevner:

1. **SQL Injection** (Manipulation af databaser gennem inputfelter)
   - Gå til login-siden.
   - Indtast følgende i brugernavnsfeltet:
     ```
     ' OR 1=1 --
     ```
   - Hvis du bliver logget ind uden adgangskode, har du udnyttet en SQL Injection-sårbarhed.

2. **Cross-Site Scripting (XSS)** (Indsprøjtning af skadelig JavaScript-kode)
   - Find et inputfelt, f.eks. søgefunktionen.
   - Indsæt følgende kode:
     ```html
     <script>alert('XSS')</script>
     ```
   - Hvis en pop-up vises, har du fundet en XSS-sårbarhed.

3. **Broken Authentication** (Utilstrækkelig beskyttelse af brugerlogin)
   - Opret en ny bruger.
   - Start Burp Suite og aktiver intercept-mode for at overvåge HTTP-trafik.
   - Prøv at logge ind med en forkert adgangskode og observer, om applikationen lækker information om kontoen i HTTP-responsen.
   - Brug Burp Suite til at manipulere login-requests og test for svage autentificeringsmekanismer.
   - Opret en ny bruger.
   - Prøv at logge ind med en forkert adgangskode og observer, om applikationen lækker information om kontoen.

4. **Directory Traversal** (Adgang til følsomme filer via URL-manipulation)
   - Start Burp Suite og aktiver intercept-mode for at overvåge HTTP-trafik.
   - Prøv at ændre URL’en til:
     ```
     http://localhost:3000/../../../../etc/passwd
     ```
   - Hvis du får adgang til systemfiler, er der en Directory Traversal-sårbarhed.
   - Brug Burp Suite til at analysere serverens svar og eksperimentere med forskellige directory traversal payloads.
   - Prøv at ændre URL’en til:
     ```
     http://localhost:3000/../../../../etc/passwd
     ```
   - Hvis du får adgang til systemfiler, er der en Directory Traversal-sårbarhed.

5. **Security Misconfiguration** (Fejlkodede sikkerhedsindstillinger)
   - Start Burp Suite og aktiver intercept-mode for at overvåge HTTP-trafik.
   - Åbn udviklerkonsollen (`F12` i browseren) og undersøg om der er eksponerede API-kald.
   - Analyser HTTP-headers og fejlmeddelelser i Burp Suite for at finde potentielle sikkerhedskonfigurationsfejl.
   - Prøv at sende ændrede HTTP-anmodninger via Burp Suite for at teste, om serveren tillader usikre konfigurationer.
   - Åbn udviklerkonsollen (`F12` i browseren) og undersøg om der er eksponerede API-kald.

6. **Broken Access Control** (Uautoriseret adgang til sider eller funktioner)

   **Hvad skal du teste?**
   - Kan en almindelig bruger tilgå funktioner, der burde være beskyttede?
   - Kan du ændre en brugers rolle via manipulerede forespørgsler?

   **Sådan gør du:**

   1. **Test adgang til admin-panel**
      - Åbn din browser og gå til:
        ```
        http://localhost:3000/admin
        ```
      - Hvis du kan få adgang uden at være logget ind som administrator, er der en fejl i adgangskontrollen.

   2. **Brug Burp Suite til at manipulere anmodninger**
      - Start Burp Suite og aktiver intercept-mode.
      - Log ind som en almindelig bruger.
      - Udfør en handling (f.eks. opdater en profil) og fang HTTP-anmodningen i Burp Suite.
      - Ændr eventuelle bruger-ID'er eller roller i anmodningen og send den igen.
      - Hvis du kan ændre data for en anden bruger eller give dig selv administratorrettigheder, er adgangskontrollen brudt.

   3. **Test adgang til en anden brugers konto**
      - Log ind som en almindelig bruger.
      - Besøg en side, hvor du kan se din egen konto, f.eks.:
        ```
        http://localhost:3000/user/123
        ```
      - Prøv at ændre ID'et i URL’en til en anden værdi, f.eks.:
        ```
        http://localhost:3000/user/124
        ```
      - Hvis du kan se en anden brugers oplysninger, er der en alvorlig sikkerhedsfejl.

   **Hvad skal du observere?**
   - Får du en fejlmeddelelse, når du forsøger at tilgå beskyttede sider?
   - Kan du ændre roller eller adgangsrettigheder gennem Burp Suite?
   - Kan du få adgang til andre brugeres data ved blot at ændre URL’en?

   Hvis svaret på nogen af ovenstående er "ja", er der en fejl i adgangskontrollen, som skal rettes.
   - Start Burp Suite og aktiver intercept-mode for at overvåge og manipulere HTTP-trafik.
   - Prøv at tilgå administratorpanelet ved at ændre URL’en til:
     ```
     http://localhost:3000/admin
     ```
   - Hvis du kan få adgang uden autorisation, er der en adgangskontrolfejl.
   - Fang en HTTP-anmodning til en beskyttet ressource i Burp Suite og ændr brugerroller i cookies, headers eller URL-parametre for at se, om du kan eskalere dine rettigheder.
   - Prøv at sende POST/PUT-forespørgsler til API-endpoints for at udføre handlinger, der kræver højere privilegier, og se om serveren validerer din adgang korrekt.
   - Log ind som en almindelig bruger og prøv at tilgå en anden brugers data ved at manipulere ID'er i URL'en eller forespørgselsparametrene.
   - Prøv at tilgå administratorpanelet ved at ændre URL’en til:
     ```
     http://localhost:3000/admin
     ```
   - Hvis du kan få adgang uden autorisation, er der en adgangskontrolfejl.

7. **CSRF (Cross-Site Request Forgery)** (Tvang en bruger til at udføre uønskede handlinger)

   **Hvad skal du teste?**
   - Kan du udføre en handling på vegne af en anden bruger uden deres viden?
   - Er applikationen sårbar over for uautoriserede anmodninger?

   **Sådan gør du:**

   1. **Identificer en formular eller handling, der ændrer data**
      - Find en side, hvor du kan udføre en ændring, f.eks. opdatering af brugeroplysninger.
      - Udfyld formularen, men **stop inden du trykker på 'Submit'.**

   2. **Fang og analyser HTTP-anmodningen**
      - Åbn udviklerkonsollen (`F12` i browseren) og gå til fanen **Netværk**.
      - Klik på 'Submit' og find den anmodning, der sendes.
      - Notér URL’en, HTTP-metoden (POST/PUT), og de data der sendes.

   3. **Simulér en CSRF-angrebsside**
      - Opret en simpel HTML-fil på din lokale maskine:
        ```html
        <html>
        <body>
        <form action="http://localhost:3000/api/endpoint" method="POST">
            <input type="hidden" name="username" value="hacker123">
            <input type="hidden" name="role" value="admin">
            <input type="submit" value="Click Me">
        </form>
        <script>
            document.forms[0].submit();
        </script>
        </body>
        </html>
        ```
      - Erstat `http://localhost:3000/api/endpoint` med den rigtige URL fra din anmodning.
      - Åbn HTML-filen i en browser og se, om anmodningen udføres.

   **Hvad skal du observere?**
   - Bliver handlingen udført uden autorisation?
   - Er der nogen CSRF-beskyttelse (f.eks. CSRF-token) i anmodningen?
   - Kan du ændre en anden brugers oplysninger uden deres samtykke?

   Hvis du kan udføre anmodningen uden en gyldig CSRF-token, har applikationen en CSRF-sårbarhed.
   - Prøv at udføre en handling på en anden brugers vegne ved at ændre en forespørgsel og sende den til applikationen.

8. **Insecure Direct Object References (IDOR)** (Manuel ændring af objektreferencer)

   **Hvad skal du teste?**
   - Kan en bruger få adgang til en anden brugers data ved at ændre URL’en?
   - Beskytter applikationen mod uautoriserede forespørgsler?

   **Sådan gør du:**

   1. **Find en side, der viser brugerens data**
      - Log ind som en almindelig bruger.
      - Gå til en profilside eller en ressource, hvor dit eget ID bruges i URL’en, f.eks.:
        ```
        http://localhost:3000/user/123
        ```

   2. **Manipulér URL’en for at prøve at få adgang til en anden brugers data**
      - Prøv at ændre ID’et i URL’en til en anden værdi, f.eks.:
        ```
        http://localhost:3000/user/124
        ```
      - Hvis du kan se en anden brugers oplysninger, har du fundet en IDOR-sårbarhed.

   3. **Brug Burp Suite til at teste yderligere**
      - Start Burp Suite og aktiver intercept-mode.
      - Fang en anmodning, hvor dit eget ID er en del af URL’en eller i anmodningens body.
      - Redigér ID’et til en anden brugers ID og send anmodningen igen.
      - Hvis du får adgang til en anden brugers data, er adgangskontrollen brudt.

   **Hvad skal du observere?**
   - Blokerer applikationen uautoriserede ændringer i ID’er?
   - Returnerer serveren en "403 Forbidden" fejl, eller får du adgang til data, du ikke burde kunne se?
   - Er der nogen form for adgangskontrol, der forhindrer dig i at hente andre brugeres oplysninger?

   Hvis du kan tilgå en anden brugers oplysninger ved blot at ændre et ID i URL’en, er der en IDOR-sårbarhed, der skal udbedres.
   - Log ind som en bruger og prøv at ændre bruger-ID'er i URL’en for at få adgang til en anden brugers data.

9. **Using Components with Known Vulnerabilities** (Brug af usikre biblioteker)

   **Hvad skal du teste?**
   - Er applikationen afhængig af forældede eller usikre tredjepartsbiblioteker?
   - Indeholder nogle af de anvendte komponenter kendte sårbarheder?

   **Sådan gør du:**

   1. **Identificér brugte komponenter**
      - Åbn udviklerkonsollen (`F12` i browseren) og gå til fanen **Netværk** eller **Kildekode**.
      - Undersøg hvilke JavaScript-, CSS- og andre eksterne biblioteker, der bliver brugt af Juice Shop.

   2. **Brug et analyseværktøj til at scanne for sårbarheder**
      - Installer `OWASP Dependency-Check` eller `npm audit` til at analysere afhængigheder:
        ```sh
        npm audit
        ```
      - Hvis du bruger Burp Suite, kan du også scanne applikationen for kendte sårbarheder.
      
   3. **Søg efter kendte sårbarheder**
      - Brug databaser som:
        - [CVE Details](https://www.cvedetails.com/)
        - [NVD - National Vulnerability Database](https://nvd.nist.gov/)
        - [Snyk](https://snyk.io/)
      - Indtast versionsnummeret på de identificerede biblioteker og tjek, om der findes kendte sårbarheder.

   **Hvad skal du observere?**
   - Bruges der forældede eller kendt usikre biblioteker?
   - Rapporterer `npm audit` eller andre værktøjer om kritiske sårbarheder?
   - Er nogle afhængigheder listet som "deprecated" eller har de opdateringer tilgængelige?

   Hvis applikationen bruger kendt usikre komponenter, bør de opdateres eller udskiftes med sikrere alternativer.
   - Brug udviklerværktøjer til at tjekke, hvilke tredjepartsbiblioteker Juice Shop anvender.

10. **Logging og Overvågning** (Manglende opsamling af sikkerhedshændelser)

   **Hvad skal du teste?**
   - Bliver angreb og uautoriserede forsøg logget korrekt?
   - Kan logs give nok information til at identificere et angreb?
   
   **Sådan gør du:**

   1. **Udfør et angreb og undersøg logs**
      - Udfør en af de tidligere øvelser (f.eks. SQL Injection eller Broken Authentication).
      - Log ind på serveren og undersøg logfilerne.
      - På Linux-systemer kan du typisk finde logs her:
        ```sh
        cat /var/log/syslog
        cat /var/log/nginx/access.log
        ```
      - Hvis angrebet ikke bliver logget, er der en manglende overvågningsmekanisme.

   2. **Brug Burp Suite til at sende flere angreb**
      - Start Burp Suite og send gentagne anmodninger med forskellige angrebsmetoder.
      - Tjek serverens respons og se, om der er fejlmeddelelser, der afslører information om systemet.
      - Se, om serveren reagerer anderledes efter flere angreb (f.eks. midlertidig blokering af IP-adresse).

   3. **Test logging af API-kald**
      - Hvis Juice Shop har en API, kan du bruge en API-klient som Postman til at sende anmodninger.
      - Test, om uautoriserede forespørgsler logges i serverens logfiler.
      - Se om fejlmeddelelser giver nok information til at identificere forsøg på angreb.

   **Hvad skal du observere?**
   - Logges der nok detaljer til at forstå og identificere angrebsforsøg?
   - Kan logs hjælpe med at identificere angriberen (IP, brugeragent osv.)?
   - Er der mekanismer til at opdage og reagere på gentagne angreb?

   Hvis applikationen ikke har tilstrækkelig logging og overvågning, kan angreb forblive uopdagede, hvilket er en alvorlig sikkerhedsrisiko.
   - Udfør et angreb og undersøg, om applikationen logger forsøget.

## Konklusion

- OWASP er afgørende for web security
- OWASP Top 10 er et must-know for udviklere
- Juice Shop giver praktisk erfaring med web security

## Spørgsmål?

- Q&A session

