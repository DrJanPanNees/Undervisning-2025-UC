---

title: "Juice Shop - PenTest øvelse (Modul 5)"
purpose: "Træne praktisk web-sårbarhedsanalyse ved at lade grupper vælge et område i OWASP Juice Shop (fx XSS, SQLi, Auth bypass) og demonstrere et fungerende exploit + mitigation."
prerequisites:
  - "Docker installeret og kørende"
  - "Burp Suite (Community eller Pro) installeret"
  - "Grundlæggende kendskab til HTTP, DevTools og JavaScript"
duration: "2 lektioner (2x90 min) eller 1 projektlektion + demo i næste time"
group_size: "2–3 studerende"
tools:
  - "Docker"
  - "Burp Suite"
  - "Browser DevTools (Chrome/Firefox)"
  - "GitHub/GitLab (valgfrit til aflevering)"
learning_outcomes:
  - "Identificere og beskrive en web-sårbarhed i Juice Shop"
  - "Demonstrere exploit via DevTools/Burp"
  - "Forklare hvorfor det er et sikkerhedshul og foreslå realistiske mitigations"
deliverables:
  - "1 slide deck (max 8 slides)"
  - "1 kort live demo (3 min) i næste lektion"
  - "1 kort md-fil med PoC steps, analyse og references (se 'PoC + analyse' skabelon)"
assessment_criteria:
  - "Valg af sårbarhed og relevans (15%)"
  - "Demonstration (30%)"
  - "Analyse – hvorfor er det et sikkerhedshul? (25%)"
  - "Mitigation – hvordan vil I lukke det? (20%)"
  - "Formidling og samarbejde (10%)"
safety_ethics:
  - "Arbejd kun mod din lokale Juice Shop-instans"
  - "Ingen scans/angreb mod eksterne/produktions-systemer"

---

# Juice Shop — Øvelse og Hurtigstart (opdateret med analyse-krav)

## Kort version — hvad skal I gøre
1. Vælg en sårbarhed i Juice Shop (fx XSS, SQLi, auth bypass).
2. I må gerne følge en ekstern guide eller tutorial, men **I skal forstå og forklare**:  
   - hvorfor koden/designet fører til et sikkerhedshul (root cause), og  
   - hvordan I realistisk vil lukke det (mitigation), inklusive eventuelle trade-offs.  
3. Demonstrer en fungerende PoC (lokalt) og upload en kort `poc_analysis.md` med jeres forklaring, payloads, screenshots og referencer.  
4. Forbered en kort 3-minutters demo i klassen hvor I viser PoC og forklarer analysen og mitigations.

---

## Læringsmål (kort)
- Forstå sårbarhedens tekniske årsag (root cause).  
- Kunne demonstrere og reproducere en PoC lokalt.  
- Kunne formulere en realistisk, teknisk mitigation og diskutere konsekvenser.

---

## Hurtigstart — rækkefølge (vigtigt)
Følg denne rækkefølge — ellers fanger Burp ikke trafikken korrekt:

1. Installer og start Burp Suite (download: https://portswigger.net/burp). Installér app'en i `/Applications` på macOS.  
2. Start Burp Suite. Vent til hovedvinduet vises.  
3. Tjek Burp Proxy listener: `Proxy → Options → Proxy listeners` — sørg for listener på `127.0.0.1:8080` og at den *kører* (Running). Overvej at slå **Support invisible proxying** til hvis I arbejder med lokale hosts.  
4. Åbn Burp’s browser fra Burp: `Proxy → Open browser`. Brug denne hvis den virker. **BRUG BURP CHROMIUM KUN HVIS DEN VIRKER.**  
5. I Burp-browseren (eller i din proxied Chrome): **ÅBN først** `http://burpsuite` og download CA-cert, hvis I vil inspecte HTTPS.  
6. Når cert'et er installeret (kun hvis nødvendigt): **ÅBN** `http://localhost:3000` i Burp-browseren eller i proxied Chrome.  
7. Gå tilbage til Burp og sæt `Proxy → Intercept = ON`. Udfør en handling i Juice Shop (fx login eller søgning) og bekræft at GET/POST vises i Intercept‑fanen.

---

## Krav til aflevering — `poc_analysis.md` (skabelon)
Jeres upload skal indeholde en `poc_analysis.md` med følgende sektioner — brug dette som skabelon og udfyld præcist:

```markdown
# Titel på PoC / Analyse
- Gruppe: Navn(e)
- Valgt sårbarhed: (fx Stored XSS)
- Kort beskrivelse (1–2 linjer)

## 1) Forudsætninger
- Juice Shop kørende på `http://localhost:3000`
- Burp kørende på `127.0.0.1:8080` (eller egen Chrome med proxy)

## 2) PoC (trin-for-trin)
- Trin 1: ... (nøjagtige klik/requests)
- Trin 2: ... (payloads, headers, body)
- Forventet resultat: ...  
- Screenshots: indsæt/vedhæft screenshots (2–3 anbefalet)

## 3) Analyse — hvorfor er det et sikkerhedshul? (MEGET VIGTIGT)
- Root cause: (hvilket stykke kode, design eller antagelse fører til sårbarheden?)  
- Angrebsoverflade: (hvilke inputs/endpoints er eksponeret?)  
- Impact: (hvad kan en angriber opnå — exfiltration, takeover, privilege escalation?)  
- Sandsynlighed: (hvor realistisk er et reelt angreb i produktion?)

## 4) Mitigation — hvordan lukker I det?
- Kort teknik-løsning (fx server-side input validation, prepared statements, output encoding, CSP).  
- Implementationsforslag (hvilke filer/komponenter skal ændres, og hvordan).  
- Trade-offs / begrænsninger (fx performance, backward compatibility).

## 5) Referencer
- Links til guides, OWASP-dokumentation, blogposts I fulgte.
```

**Obs:** Hvis I følger en online guide, så medtag linket i Referencer og skriv i Analyse-sektionen hvilke dele I ændrede eller forstod ud over guiden.

---

## Intercept‑test (hurtig)
- Sæt `Proxy → Intercept = On` i Burp.  
- Udfør handling i Juice Shop (fx login eller søg).  
- I Intercept-fanen skal I kunne se GET/POST. Forward request eller send til Repeater/Logger for videre analyse.

---

## Kort oversigt: HTTP‑metoder (cheat‑sheet)
**GET** — Hent ressourcer/data. Idempotent. Parametre i URL (query).  
**POST** — Send data for at oprette/ændre ressourcer. Ikke‑idempotent. Body indeholder data (JSON/form).  
**PUT** — Erstat en ressource fuldstændigt. Idempotent.  
**PATCH** — Delvis opdatering.  
**DELETE** — Slet en ressource. Idempotent.  
**HEAD** — Som GET, kun headers.  
**OPTIONS** — Spørg server hvilke metoder/resource understøtter (CORS preflight).

---

## Bedømmelse (opdateret vægtning)
- **Valg af sårbarhed og relevans (15%)**  
- **Demonstration (30%)**  
- **Analyse — hvorfor er det et sikkerhedshul? (25%)**  
- **Mitigation — løsning og realism (20%)**  
- **Formidling og samarbejde (10%)**

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

## Checklist til underviseren (print/slide)
- [ ] Docker + Juice Shop OK på maskinerne  
- [ ] Burp listener kører på 127.0.0.1:8080 (eller alternativ port)  
- [ ] **Burp Chromium: virker? BRUG KUN HVIS DEN VIRKER — ELLERS START EGEN CHROME MED PROXY**  
- [ ] Bed eleverne indsende `poc_analysis.md` og slides  
- [ ] Etik/guidelines gennemgået

---

## Ekstra tips til undervisningen
- Giv et kort eksempel (5 min) fra din egen maskine: find et simpelt XSS og vis hvordan I forklarer root cause + mitigation.  
- Bed grupperne dokumentere hvad de prøvede — ikke kun hvad der virkede.  
- Hav 1–2 hjælpere klar til Burp/ports og Docker i starten.  
