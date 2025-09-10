
# Øvelse: Installation og brug af MySQL og Nginx i Linux Mint

**Formål:**  
At kunne installere og bruge MySQL og Nginx lokalt på Linux Mint, oprette en database med tabeller relateret til semesterprojektet *Maleren A/S*, og hoste en simpel HTML-side med Nginx.

---

## 🌐 Del 4: Installer og opsæt Nginx med en HTML-side

1. Installer Nginx:
```
sudo apt install nginx
```

2. Start og aktiver Nginx:
```
sudo systemctl start nginx
sudo systemctl enable nginx
```

3. Tjek om det virker ved at åbne browser og gå til:
```
http://localhost
```

Du burde se en "Welcome to nginx"-side.

4. Erstat standard HTML-side:
   - Gå til webroden:
     ```
     cd /var/www/html/
     ```
   - Åbn filen med:
     ```
     sudo nano index.html
     ```
   - Ændr nu siden til noget andet. Fx:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Maleren A/S</title>
</head>
<body>
    <h1>Velkommen til Maleren A/S</h1>
    <p>Din professionelle partner i maling og værktøj.</p>
    <a href="om.html">Læs mere om os</a>
</body>
</html>
```

5. Udfordring:
   - Opret endnu en side med:
     ```
     sudo nano om.html
     ```
   - Tilføj følgende:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Om os</title>
</head>
<body>
    <h1>Om Maleren A/S</h1>
    <p>Vi har mere end 30 års erfaring med maling og rådgivning.</p>
    <a href="index.html">Tilbage til forsiden</a>
</body>
</html>
```

6. Gå igen til:
```
http://localhost
```

Du burde nu kunne navigere frem og tilbage mellem de to sider.
