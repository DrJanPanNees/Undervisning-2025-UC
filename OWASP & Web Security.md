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

# Juice Shop — Øvelse og Hurtigstart

## Læringsmål
- Forstå og demonstrere en web-sårbarhed i en lokal instans af OWASP Juice Shop.  
- Fremvise en fungerende proof-of-concept (PoC).  
- Kort beskrive realistiske mitigationsforslag.

---

## Hurtigstart — kør Juice Shop lokalt (5 min)
Kør disse kommandoer i Terminal:
```bash
docker pull bkimminich/juice-shop
docker run --rm -p 3000:3000 bkimminich/juice-shop
