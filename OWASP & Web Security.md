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

### Opsætning af Juice Shop i en Docker-container

1. Åbn en terminal
2. Kør følgende kommando for at downloade og starte Juice Shop:
   ```sh
   docker run -d -p 3000:3000 bkimminich/juice-shop
   ```
3. Åbn en browser og gå til `http://localhost:3000`
4. Juice Shop er nu klar til brug

## Hands-on – Øvelser

### Identificer og udnyt sikkerhedshuller i Juice Shop

Her er nogle praktiske øvelser, hvor du kan teste dine sikkerhedsevner:

1. **SQL Injection**: Forsøg at logge ind med `' OR 1=1 --`
2. **Cross-Site Scripting (XSS)**: Indsæt `<script>alert('XSS')</script>` i inputfelter
3. **Broken Authentication**: Find måder at omgå login-siden på
4. **Directory Traversal**: Prøv at tilgå skjulte filer via URL-manipulation
5. **Security Misconfiguration**: Find usikre API-endepunkter
6. **Broken Access Control**: Forsøg at få adgang til administratorfunktioner
7. **CSRF (Cross-Site Request Forgery)**: Udnyt sessionshåndtering
8. **Insecure Direct Object References (IDOR)**: Manipuler ID'er i URL’er
9. **Using Components with Known Vulnerabilities**: Undersøg brugte bibliotekers sikkerhed
10. **Logging og Overvågning**: Se om angreb bliver logget korrekt

## Konklusion

- OWASP er afgørende for web security
- OWASP Top 10 er et must-know for udviklere
- Juice Shop giver praktisk erfaring med web security

## Spørgsmål?

- Q&A session

