## 🧪 Ekstra øvelse: Scale Cube i praksis

Denne øvelse viser, hvordan du i praksis kan afprøve X- og Y-dimensionerne i Scale Cube med Docker Compose.

---

### 📁 Step 1: Opret mappestruktur

```bash
mkdir ScaleCubeDemo
cd ScaleCubeDemo
mkdir KundeService ProduktService
```

📌 *Metadata:* Vi starter med at opdele vores services (Y-skala) i hver sin mappe.

---

### 🐳 Step 2: Tilføj Dockerfile til begge services

**KundeService/Dockerfile** og **ProduktService/Dockerfile**

```dockerfile
FROM alpine
CMD while true; do echo "Service running..."; sleep 5; done
```

📌 *Metadata:* Vi bruger Alpine for hurtig opstart og skriver dummy-output til konsollen.

---

### ⚙️ Step 3: Tilføj `docker-compose.yml`

I roden af `ScaleCubeDemo`:

```yaml
version: '3.9'

services:
  kunde:
    build: ./KundeService
    deploy:
      replicas: 3  # X-skalering: 3 instanser af KundeService

  produkt:
    build: ./ProduktService
    deploy:
      replicas: 2  # X-skalering: 2 instanser af ProduktService
```

📌 *Metadata:* Compose-konfigurationen illustrerer både X (replicas) og Y (flere services).

---

### 🚀 Step 4: Kør løsningen

```bash
docker-compose up --scale kunde=3 --scale produkt=2
```

📌 *Metadata:* Du starter nu 5 containere – 3 for kunde og 2 for produkt. Brug `docker ps` for at se dem køre.

---

### 🔍 Step 5: Bekræft resultatet

```bash
docker ps
```

Forvent output som:

```
scalecubedemo_kunde_1
scalecubedemo_kunde_2
scalecubedemo_kunde_3
scalecubedemo_produkt_1
scalecubedemo_produkt_2
```

📌 *Metadata:* Dette demonstrerer skalering i praksis og synliggør konceptet fra Scale Cube-modellen.

---

### 🎓 Perspektivering

- X = Flere containere af samme service → Docker replicas
- Y = Flere services → Én container per ansvar
- Z = Ikke dækket her, men ville kræve dataopdeling (sharding)