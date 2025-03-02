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
   - Hent data:
     ```bash
     curl -X GET http://localhost:3000/posts
     ```
   - Opret en ny ressource:
     ```bash
     curl -X POST http://localhost:3000/posts -H "Content-Type: application/json" -d '{"title": "Ny Post", "author": "Jan"}'
     ```
   - Opdater en ressource:
     ```bash
     curl -X PUT http://localhost:3000/posts/1 -H "Content-Type: application/json" -d '{"title": "Opdateret Post", "author": "Jan"}'
     ```
   - Slet en ressource:
     ```bash
     curl -X DELETE http://localhost:3000/posts/1
     ```
4. Stop og fjern API'en:
   ```bash
   docker compose down
   ```

