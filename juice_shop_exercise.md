# Juice Shop - PenTest øvelse (Modul 5)

| Felt | Værdi |
|---|---|
| **Title** | Juice Shop - PenTest øvelse (Modul 5) |
| **Purpose** | Træne praktisk web-sårbarhedsanalyse ved at lade grupper vælge et område i OWASP Juice Shop (fx XSS, SQLi, Auth bypass) og demonstrere et fungerende exploit + mitigering. |
| **Prerequisites** | Docker installeret og kørende; Burp Suite (Community eller Pro) installeret; Grundlæggende kendskab til HTTP, DevTools og JavaScript |
| **Duration** | 2 lektioner (2x90 min) eller 1 projektlektion + demo i næste time |
| **Group size** | 2–3 studerende |
| **Tools** | Docker, Burp Suite, Browser DevTools (Chrome/Firefox), GitHub/GitLab (valgfrit) |
| **Learning outcomes** | Identificere og beskrive en web-sårbarhed; Demonstrere exploit; Beskrive mitigations |
| **Deliverables** | 1 slide deck (max 8 slides); 1 kort live demo (3 min); 1 kort md-fil med PoC |
| **Assessment criteria** | Valg af sårbarhed 20%; Demonstration 40%; Mitigation 25%; Formidling 15% |
| **Safety / Ethics** | Arbejd kun mod din lokale Juice Shop-instans; Ingen angreb mod eksterne systemer |

---

## Juice Shop — Øvelse og Hurtigstart

### Læringsmål
- Forstå og demonstrere en web-sårbarhed i en lokal instans af OWASP Juice Shop.  
- Fremvise en fungerende proof-of-concept (PoC).  
- Kort beskrive realistiske mitigationsforslag.

---

### Hurtigstart — kør Juice Shop lokalt (5 min)
Kør disse kommandoer i Terminal:
```bash
docker pull bkimminich/juice-shop
docker run --rm -p 3000:3000 bkimminich/juice-shop
```
Åbn i browser: `http://localhost:3000`

---

### Burp / browseropsæt (anbefaling til undervisning)

> **VIGTIGT — LÆS FØRST (KORT):**  
> **BRUG BURP’S INDLEGGEREDE CHROMIUM KUN HVIS DEN VIRKER FOR DIG.**  
> **HVIS BURP CHROMIUM IKKE VIRKER STABILT, BRUG DIN EGEN CHROME MED PROXY-FLAG I STEDET.**

- Hvis Burp Chromium virker hos dig: brug den (i Burp: *Proxy → Open browser*).  
- **Hvis Burp Chromium fejler (fx `ERR_INVALID_REDIRECT` eller `ERR_TUNNEL_CONNECTION_FAILED`) — START DIN EGEN CHROME med proxy-flag:**
  ```bash
  open -a "Google Chrome" --args --proxy-server="http://127.0.0.1:8080" --user-data-dir="/tmp/chrome-burp"
  ```
- I Burp: *Proxy → Options* → sørg for en listener `127.0.0.1:8080` (Running). Overvej at slå **Support invisible proxying** til ved arbejde med lokale hosts.
- For HTTPS-inspektion: i Burp → *Proxy → Open browser* → gå til `http://burpsuite` → download CA-cert og importer i macOS Keychain som trusted (kun hvis I skal inspicere HTTPS).

**Gentagelse så de ikke misser det:**  
**BRUG BURP CHROMIUM KUN HVIS DEN VIRKER. ELLERS START DIN EGEN CHROME** — det er ofte den hurtigste løsning for undervisning.

---

### Valg af emne (eksempler)
- Reflected / Stored / DOM XSS  
- SQL Injection (login, search)  
- Authorization bypass / JWT manipulation  
- CSRF, Open redirect, SSRF (hvis opgaverne findes i app’en)

---

### Arbejdsgang (forslag)
- 0–10 min: Recon med DevTools (Network, Elements, Console). Find endpoints og inputs.  
- 10–30 min: Simpel PoC via DevTools / Burp Repeater (fx payload for XSS).  
- 30–45 min: Beskriv mitigation: hvad skal ændres i kode/config.  
- 45–60 min: Forbered slide/demo (hvad, hvordan, hvorfor, mitigation).

---

### Aflevering (format og krav)
- Upload én .zip med: `slides.pdf` (max 8 slides), `poc.md` (trin-for-trin), evt. 2–3 screenshots.  
- I klassen: 3 min live demo + 2 min Q&A.  
- Deadline: næste lektion (eller angivet ved opgaveudlevering).

---

### Etiske regler
- Angrib kun din lokale Juice Shop-instans.  
- Ingen scanning eller angreb mod eksterne/produktionssystemer.  
- Del ikke payloads som kan misbruges uden kontekst/etisk ansvar.

---

## Bedømmelse (kort rubrik)
- **Valg af sårbarhed (20%)**: Relevans og begrundelse.  
- **Demonstration (40%)**: Fungerende PoC og forklaring.  
- **Mitigation (25%)**: Realistiske, korrekte forslag.  
- **Formidling (15%)**: Klar præsentation og samarbejde.

---

## Troubleshooting — vitale kommandoer (til slide)
Sæt disse på en "quick fixes" slide; de hjælper de fleste fra start:

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

**Tip:** Hvis en Docker container bruger port 8080, stop den, eller skift Burp til en anden port. Port-konflikter er en almindelig årsag til fejl i Burp Chromium.

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
