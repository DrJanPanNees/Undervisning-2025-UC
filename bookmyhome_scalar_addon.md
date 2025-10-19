# Øvelse: Scale Cube i praksis — BookMyHome Auth Add-on

## 🎬 Kontekst
Jeres *BookMyHome*-projekt er i gang.  
I har allerede en eller flere services — fx `AuthService`, `BookingService`, `HomeService` eller lignende.  
Nu kommer jeres Product Owner med en ny udfordring:

> “Vi får flere brugere! Systemet skal kunne håndtere vækst uden at gå ned.”  

I skal derfor arbejde med **skalering i lille skala**:  
Hvordan kan jeres nuværende system gøres mere robust, hurtigere og nemmere at udvide?

---

## 🎯 Formål
At give jer en forståelse for **Scale Cube-modellen**:
- **X-skala:** Replikering (flere instanser af samme service)
- **Y-skala:** Funktionsopdeling (flere uafhængige services)
- **Z-skala:** Data- eller brugeropdeling (segmentering)

---

## 🧩 Læringsmål
| Område | Mål |
|--------|-----|
| **Arkitektur** | Forstå hvordan et system kan vokse uden at blive omskrevet |
| **Teknologi** | Kunne tilføje replicas, dele services og data i Docker Compose |
| **Refleksion** | Vurdere fordele og ulemper ved de tre skaleringsformer |

---

## 📦 Forudsætninger
- Et kørende projekt (fx BookMyHome eller AuthService fra tidligere)
- Docker fungerer lokalt
- I forstår grundlæggende servicekommunikation (fx via Gateway eller API-kald)

---

## 🧱 Opgave: “Scalar Add-on”

### Step 1 – Forstå jeres nuværende skala
Lav en kort analyse af jeres projekt:
- Hvor mange services har I?  
- Har hver service sin egen database?  
- Hvilken del ville bryde først, hvis 1000 brugere loggede ind samtidig?

> **Tip:** Diskutér om jeres nuværende setup repræsenterer en Y-skala (funktionsopdeling).

**Metadata:**  
- *Færdighed:* At kunne identificere eksisterende flaskehalse.  
- *Refleksion:* Hvordan adskiller det sig fra en monolit?

---

### Step 2 – X-skalering (replikering)
Tilføj replikering på én af jeres services i `docker-compose.yml`:

```yaml
  authenticationservice:
    build: ./AuthenticationService
    deploy:
      replicas: 2
    ports:
      - "5000"
```

Kør systemet og observer:
- Kan Gateway’en (YARP) stadig finde jeres service?  
- Hvad sker der i loggen — fordeles kald mellem instanserne?

> **Bonus:** Tilføj et `/ping`-endpoint for at se hvilken instans, der svarer.

**Metadata:**  
- *Færdighed:* Forstå hvordan load balancing virker via proxy.  
- *Refleksion:* Hvornår giver X-skala mening, og hvornår er det spild af ressourcer?

---

### Step 3 – Y-skalering (opdeling)
Overvej om jeres projekt kan opdeles i flere funktionelle services:
- Fx `AuthService`, `BookingService`, `HomeService`.  
- Hver service skal have sit eget ansvar og egen database.  

I skal ikke nødvendigvis implementere det — det vigtigste er at tegne og beskrive, hvordan I ville gøre det.

**Hint:** Brug et simpelt arkitekturdiagram (fx draw.io, Excalidraw eller ASCII).  

**Metadata:**  
- *Færdighed:* Visualisere funktionel opdeling.  
- *Refleksion:* Hvordan påvirker dette jeres kodebaser og teamsamarbejde?

---

### Step 4 – Z-skalering (data-deling)
Diskutér i gruppen:
- Hvordan kunne I opdele brugere eller data geografisk eller logisk?  
  Fx:
  - én database for “hosts”, en anden for “guests”  
  - eller databaser fordelt på region (Danmark, Tyskland, Sverige)

> I skal ikke kode det — kun beskrive hvordan det kunne fungere i jeres system.

**Metadata:**  
- *Færdighed:* Se koblingen mellem domæne og data.  
- *Refleksion:* Hvilke nye udfordringer opstår (data-sync, konsistens)?

---

### Step 5 – Refleksion og dokumentation
Lav en kort opsamling (1-2 sider eller i README):
- Hvordan skalerer jeres system i dag?  
- Hvilke dimensioner (X, Y, Z) kunne I indføre?  
- Hvad ville det kræve teknisk?  
- Hvilke risici ville følge med?

**Bonus:**  
Tegn Scale Cube for jeres projekt som tre niveauer:
```
       ┌───────────────┐
       │   X: Copies   │  (flere instanser)
       ├───────────────┤
       │   Y: Services │  (flere domæner)
       ├───────────────┤
       │   Z: Data     │  (flere databaser)
       └───────────────┘
```

---

## 💬 Aflevering
- Kort rapport eller markdown med jeres refleksion og evt. diagrammer.  
- Hvis muligt, vis docker-logs der demonstrerer X-skala i praksis.  

**Vurdering:**  
- 50 % forståelse og refleksion  
- 30 % teknisk afprøvning (compose-opsætning)  
- 20 % dokumentation og formidling

---

## 🔎 Metadata for øvelsen
- *Tid:* 1½–2 timer  
- *Tema:* Skalering i microservice-inspireret arkitektur  
- *Fokus:* Forstå X/Y/Z-dimensionerne gennem eget projekt  
- *Output:* Tekst, diagram eller docker-demo  
- *Sværhedsgrad:* Mellem — alle kan være med uanset projektstatus
