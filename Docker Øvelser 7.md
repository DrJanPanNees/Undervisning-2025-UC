# Docker Øvelser

---

## Øvelse 7: Opsætning og test af en REST API
**Metadata:**
- **Emne:** REST API med Docker
- **Mål:** Lære at opsætte og teste en REST API-container
- **Relevans:** Grundlæggende viden om REST-arkitektur og API-interaktioner

**Instruktioner:**
1. Opret en `docker-compose.yaml` fil til en simpel REST API med JSON-server:
   ```yaml
   version: '3'
   services:
     api:
       image: clue/json-server
       ports:
         - "3000:80"
   ```
2. Start REST API-serveren:
   ```bash
   docker compose up -d
   ```
3. Test API'en med `curl` eller en API-klient (f.eks. Postman):
   - Hent alle data:
     ```bash
     curl -X GET http://localhost:3000/posts
     ```
   - Opret en ny post:
     ```bash
     curl -X POST http://localhost:3000/posts -H "Content-Type: application/json" -d '{"id": 1, "title": "Ny Post", "author": "Jan"}'
     ```
   - Opdater en eksisterende post:
     ```bash
     curl -X PUT http://localhost:3000/posts/1 -H "Content-Type: application/json" -d '{"title": "Opdateret Post", "author": "Jan"}'
     ```
   - Slet en post:
     ```bash
     curl -X DELETE http://localhost:3000/posts/1
     ```
4. Verificér ændringer ved at køre GET-kommandoen igen.
5. Stop og fjern API'en:
   ```bash
   docker compose down
   ```
6. Reflekter over hvordan API’en håndterer ændringer, og hvilke kommandoer der kunne være nyttige i en produktionssammenhæng.

