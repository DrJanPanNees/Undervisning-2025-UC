---
title: "Juice Shop - PenTest øvelse (Modul 5)"
purpose: "Træne praktisk web-sårbarhedsanalyse ved at lade grupper vælge et område i OWASP Juice Shop (fx XSS, SQLi, Auth bypass) og demonstrere et fungerende exploit + mitigering."
prerequisites:
  - Docker installeret og kørende
  - Burp Suite (Community eller Pro) installeret
  - Grundlæggende kendskab til HTTP, DevTools og JavaScript
duration: "2 lektioner (2x90 min) eller 1 projektlektion + demo i næste time"
group_size: "2–3 studerende"
tools:
  - Docker
  - Burp Suite
  - Browser DevTools (Chrome/Firefox)
  - GitHub/GitLab (valgfrit til aflevering)
learning_outcomes:
  - Identificere og beskrive en web-sårbarhed i Juice Shop
  - Demonstrere exploit via DevTools/Burp
  - Forklare hvordan sårbarheden kan afbødes (kort)
deliverables:
  - 1 slide deck (max 8 slides)
  - 1 kort live demo (3 min) i næste lektion
  - 1 kort md-fil med PoC steps og referencer
assessment_criteria:
  - Valg af sårbarhed og relevans (20%)
  - Korrekt demonstration/exploit (40%)
  - Refleksion og mitigering (25%)
  - Formidling og samarbejde (15%)
safety_ethics:
  - Arbejd kun mod din lokale Juice Shop-instans
  - Ingen scans/angreb mod eksterne/produktions-systemer
---

# Juice Shop — Øvelse og Hurtigstart (opdateret rækkefølge)

## Læringsmål
- Forstå og demonstrere en web-sårbarhed i en lokal instans af OWASP Juice Shop.  
- Fremvise en fungerende proof-of-concept (PoC).  
- Kort beskrive realistiske mitigationsforslag.

---

## Hurtigstart — rækkefølge (vigtigt)
Følg denne rækkefølge — ellers fanger Burp ikke trafikken korrekt:

1. **Installer Burp Suite** (Community eller Pro) hvis ikke allerede gjort. Download: https://portswigger.net/burp. Installér app'en i `/Applications` på macOS.  
2. **Start Burp Suite**. Vent til hovedvinduet vises.  
3. **Tjek Burp Proxy listener**: `Proxy → Options → Proxy listeners` — sørg for at der er en listener på `127.0.0.1:8080` og at den *kører* (Running). Overvej at slå **Support invisible proxying** til hvis I arbejder med lokale hosts.  
4. **Åbn Burp’s browser fra Burp**: `Proxy → Open browser` (den indbyggede Chromium). Brug denne hvis den virker. **BRUG BURP CHROMIUM KUN HVIS DEN VIRKER** — se alternativ kort nedenfor.  
5. **I den browser som Burp åbnede (eller i din egen Chrome, hvis du bruger den):** åbn nu `http://localhost:3000`. (Første gang du skal bruge Burp-browseren: gå til `http://burpsuite` for at hente CA-cert, hvis du vil inspicere HTTPS).  
6. **Gå tilbage til Burp** og sæt `Proxy → Intercept` = ON for at fange requests. Prøv at udføre en handling i Juice Shop (fx login eller søgning) og se at GET og POST requests dukker op i Intercept-fanen.  

> **Gentagelse så de ikke misser det:**  
> **BRUG BURP CHROMIUM KUN HVIS DEN VIRKER.** Hvis Burp Chromium fejler (fx `ERR_INVALID_REDIRECT`), start i stedet din egen Chrome med proxy-flag (kommando længere nede).

---

## Hvis Burp Chromium driller — hurtig alternativ (én linje)
Start din egen Chrome som kun denne gang bruger Burp-proxyen:
```bash
open -a "Google Chrome" --args --proxy-server="http://127.0.0.1:8080" --user-data-dir="/tmp/chrome-burp"
```

---

## Hurtigstart — kør Juice Shop lokalt (5 min)
Kør disse kommandoer i Terminal (før eller efter du har startet Burp — bare sørg for Burp kører før du åbner siden i browseren):
```bash
docker pull bkimminich/juice-shop
docker run --rm -p 3000:3000 bkimminich/juice-shop
```
Når Burp-browseren er åben og Intercept er ON: **ÅBN** `http://localhost:3000` i den Burp-browser eller i din proxied Chrome.

---

## Intercept-test (hurtig)
- Sæt `Proxy → Intercept = On` i Burp.  
- Udfør en simpel handling i Juice Shop (fx åbn produktliste eller klik login).  
- Du skal nu se en HTTP-request i Burp Intercept-fanen. Prøv at Forward eller Drop requesten og se effekten.  
- Skift Intercept Off og brug Repeater/Logger for at afspille eller inspicere trafik.

---

## Kort oversigt: HTTP-metoder (hurtig reference)
Her er en kort, pædagogisk oversigt du kan give de studerende som cheat-sheet:

- **GET**  
  - Formål: Hent ressourcer/data fra serveren.  
  - Karakteristika: Idempotent, ingen body typisk, parametre i URL (query string).  
  - Eksempel: `GET /products?search=soap HTTP/1.1`

- **POST**  
  - Formål: Send data til serveren for at oprette eller ændre ressourcer (fx formularindsendelse).  
  - Karakteristika: Ikke-idempotent (gentagne POST kan oprette flere ressourcer), indhold i body (JSON, form-data).  
  - Eksempel: `POST /login` med body `{ "username":"alice","password":"x" }`

- **PUT**  
  - Formål: Erstat en ressource fuldstændigt (opdatering).  
  - Karakteristika: Idempotent (gentagne PUT med samme body giver samme tilstand).  
  - Eksempel: `PUT /users/123` med body med alle felter.

- **PATCH**  
  - Formål: Delvis opdatering af en ressource.  
  - Karakteristika: Ikke nødvendigvis idempotent, bruges til ændringer af enkelte felter.

- **DELETE**  
  - Formål: Slet en ressource.  
  - Karakteristika: Idempotent (sletning af en allerede slettet ressource bør ikke ændre tilstand).

- **HEAD**  
  - Formål: Ligesom GET men kun returnerer headers (ingen body). God til at tjekke om en ressource findes eller content-length.

- **OPTIONS**  
  - Formål: Spørger server hvilke metoder og features en ressource understøtter (CORS preflight osv).

**Hvor det er relevant i øvelsen:** brug GET til at inspicere endpoints og POST/PUT/PATCH/DELETE når I tester input-validering, login eller state-changing funktioner.

---

## Valg af emne (eksempler)
- Reflected / Stored / DOM XSS  
- SQL Injection (login, search)  
- Authorization bypass / JWT manipulation  
- CSRF, Open redirect, SSRF (hvis opgaverne findes i app’en)

---

## Arbejdsgang (forslag)
- 0–10 min: Recon med DevTools (Network, Elements, Console). Find endpoints og inputs.  
- 10–30 min: Simpel PoC via DevTools / Burp Repeater (fx payload for XSS).  
- 30–45 min: Beskriv mitigation: hvad skal ændres i kode/config.  
- 45–60 min: Forbered slide/demo (hvad, hvordan, hvorfor, mitigation).

---

## Aflevering (format og krav)
- Upload én .zip med: `slides.pdf` (max 8 slides), `poc.md` (trin-for-trin), evt. 2–3 screenshots.  
- I klassen: 3 min live demo + 2 min Q&A.  
- Deadline: næste lektion (eller angivet ved opgaveudlevering).

---

## Etiske regler
- Angrib kun din lokale Juice Shop-instans.  
- Ingen scanning eller angreb mod eksterne/produktionssystemer.  
- Del ikke payloads som kan misbruges uden kontekst/etisk ansvar.

---

## Troubleshooting — vitale kommandoer (til slide)
```bash
# Hvad lytter på port 8080?
lsof -nP -iTCP:8080 -sTCP:LISTEN

# Test loopback til Burp
nc -vz 127.0.0.1 8080

# Test proxy-forwarding via Burp
curl -v -x http://127.0.0.1:8080 http://example.com/ 2>&1 | head -n 25

# Test Juice Shop lokalt
curl -v http://127.0.0.1:3000/ 2>&1 | head -n 10

# Hvis en Docker container binder 8080
docker ps --filter "publish=8080" --format "table {{.ID}}\t{{.Image}}\t{{.Ports}}"
docker stop <container-id>
```

---

## Skabelon: `poc.md` (til studerende)
Studerende skal bruge denne skabelon ved upload:

```markdown
# Titel på PoC
- Gruppe: Navn(e)
- Valgt sårbarhed: (fx Stored XSS)

## 1) Kort beskrivelse
(2–3 linjer om hvad der blev testet)

## 2) Forudsætninger
- Juice Shop kørende på `http://localhost:3000`
- Burp kørende på `127.0.0.1:8080` (eller brug egen Chrome med proxy)

## 3) Trin-for-trin PoC
1. Åbn DevTools / Burp.  
2. Gør X, Y, Z... (præcise payloads og kommandoer).  
3. Forventet resultat: (hvad skete).

## 4) Mitigation
(Kort beskrivelse af hvordan dette kan fixes: input-sanitization, prepared statements, CSP osv.)

## 5) Screenshots / logs
- Indsæt 2–3 relevante screenshots eller links.

## 6) Referencer
- Links til OWASP, Juice Shop docs eller relevante artikler.
```

---

## Checklist til underviseren (print/slide)
- [ ] Docker + Juice Shop OK på maskinerne  
- [ ] Burp listener kører på 127.0.0.1:8080 (eller alternativ port)  
- [ ] **Burp Chromium: virker? BRUG KUN HVIS DEN VIRKER — ELLERS START EGEN CHROME MED PROXY**  
- [ ] CA-cert importeret hvis I skal inspecte HTTPS  
- [ ] Etik/guidelines gennemgået

---

## Ekstra tips til undervisningen
- Del grupperne på forskelligt område, undgå overlap.  
- Hav 1–2 hjælpere klar til at hjælpe med Burp/port og Docker-fejl i starten.  
- Bed grupperne dokumentere hvad de *prøvede* (ikke kun hvad der virkede) — læring ligger i fejlsøgningen.  
- Overvej at give en kort demo fra underviserens maskine først (5 min), så eleverne ser forventet setup.
