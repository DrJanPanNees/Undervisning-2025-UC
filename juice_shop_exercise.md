# Juice Shop — Øvelse og Hurtigstart (rettet metadata-visning)

**Title:** Juice Shop - PenTest øvelse (Modul 5)  
**Purpose:** Træne praktisk web-sårbarhedsanalyse ved at lade grupper vælge et område i OWASP Juice Shop (fx XSS, SQLi, Auth bypass) og demonstrere et fungerende exploit + mitigering.  
**Prerequisites:** Docker installeret og kørende; Burp Suite (Community eller Pro) installeret; Grundlæggende kendskab til HTTP, DevTools og JavaScript.  
**Duration:** 2 lektioner (2x90 min) eller 1 projektlektion + demo i næste time.  
**Group size:** 2–3 studerende.  
**Tools:** Docker, Burp Suite, Browser DevTools (Chrome/Firefox), GitHub/GitLab (valgfrit).  
**Learning outcomes:** Identificere og beskrive en web-sårbarhed; Demonstrere exploit; Beskrive mitigations.  
**Deliverables:** slides.pdf (max 8 slides), poc.md (trin-for-trin), kort live demo (3-10 min).  
**Assessment criteria:** Valg af sårbarhed 20%; Demonstration 40%; Mitigation 25%; Formidling 15%.  
**Safety/Ethics:** Arbejd kun mod lokal Juice Shop; ingen angreb mod eksterne systemer.

---

## Læringsmål
- Forstå og demonstrere en web-sårbarhed i en lokal instans af OWASP Juice Shop.  
- Fremvise en fungerende proof‑of‑concept (PoC).  
- Kort beskrive realistiske mitigationsforslag.

---

## Hurtigstart — rækkefølge (vigtigt)
Følg denne rækkefølge — ellers fanger Burp ikke trafikken korrekt:

1. **Installer Burp Suite** (Community eller Pro) hvis ikke allerede gjort. Download: https://portswigger.net/burp. Installér app'en i `/Applications` på macOS.  
2. **Start Burp Suite**. Vent til hovedvinduet vises.  
3. **Tjek Burp Proxy listener:** `Proxy → Options → Proxy listeners` — sørg for en listener på `127.0.0.1:8080` og at den *kører* (Running). 
4. **Gå tilbage til Burp** og sæt `Proxy → Intercept` = ON for at fange requests. Prøv at udføre en handling i Juice Shop (fx login eller søgning) og se at GET og POST requests dukker op i Intercept‑fanen.

> **Gentagelse så de ikke misser det:**  
> **BRUG BURP CHROMIUM HVIS DEN VIRKER.** Hvis Burp Chromium fejler (fx `ERR_INVALID_REDIRECT`), start i stedet din egen Chrome med proxy-flag (kommando længere nede).

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

## Intercept‑test (hurtig)
- Sæt `Proxy → Intercept = On` i Burp.  
- Udfør en simpel handling i Juice Shop (fx åbn produktliste eller klik login).  
- Du skal nu se en HTTP‑request i Burp Intercept‑fanen. Prøv at Forward eller Drop requesten og se effekten.  
- Skift Intercept Off og brug Repeater/Logger for at afspille eller inspicere trafik.

---

## Kort oversigt: HTTP‑metoder (cheat‑sheet)
**GET** — Hent ressourcer/data fra serveren. Idempotent. Parametre i URL (query).  
**POST** — Send data for at oprette/ændre ressourcer. Ikke‑idempotent. Body indeholder data (JSON/form).  
**PUT** — Erstat en ressource fuldstændigt. 
**PATCH** — Delvis opdatering af en ressource.  
**DELETE** — Slet en ressource. 
**HEAD** — Som GET, men kun headers.  
**OPTIONS** — Spørg server hvilke metoder/features en resource understøtter (CORS preflight).

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
- Upload én .zip med: `slides.pdf` (max 8 slides), `poc.md` (trin‑for‑trin), evt. 2–3 screenshots.  
- I klassen: 3-10 min live demo + 2 min Q&A.  
- Deadline: næste lektion (eller angivet ved opgaveudlevering).

---

## Etiske regler
- Angrib kun din lokale Juice Shop‑instans.  
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

## Skabelon:
I skal bruge denne skabelon ved upload:

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

