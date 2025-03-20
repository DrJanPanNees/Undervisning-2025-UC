# Docker Compose: Guidet Øvelse - Webapplikation med MySQL

## **Indledning**
Dette er en guidet øvelse, hvor vi opsætter en **webapplikation**, som skriver data til en **MySQL-database** ved navn **kundeDB**. Vi bruger **PHP** og **MySQL** til dette setup.

---

## **📚 Opgave: Opret en webapplikation med MySQL**

I denne øvelse vil vi:
- Opsætte en **PHP-webserver**.
- Oprette en **MySQL-database**.
- Lave et simpelt **webinterface**, hvor brugeren kan indsætte og hente data fra databasen.

---

### **1️⃣ Opret projektmappen**
```sh
mkdir docker-compose-php-mysql
cd docker-compose-php-mysql
```

---

### **2️⃣ Opret `docker-compose.yml`**
```yaml
version: "3.8"

services:
  web:
    image: php:8.1-apache
    container_name: php-web
    volumes:
      - ./app:/var/www/html
    ports:
      - "8080:80"
    depends_on:
      - database

  database:
    image: mysql:8
    container_name: mysql-db
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
      MYSQL_DATABASE: ${MYSQL_DATABASE}
      MYSQL_USER: ${MYSQL_USER}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD}
    volumes:
      - db_data:/var/lib/mysql
    ports:
      - "3306:3306"

volumes:
  db_data:
```

Dette opsætter:
- En **PHP-webserver**, der kører **Apache**.
- En **MySQL-database**, der gemmer data ved hjælp af et **Docker volume**.

---

### **3️⃣ Opret en `.env` fil**
Opret en **`.env`** fil i projektmappen:
```ini
MYSQL_ROOT_PASSWORD=rootpassword
MYSQL_DATABASE=kundeDB
MYSQL_USER=user
MYSQL_PASSWORD=password
```
Dette sikrer, at MySQL-containeren starter med de rigtige værdier.

---

### **4️⃣ Opret PHP-applikationen**
#### **Opret `app/index.php`**
```sh
mkdir app
nano app/index.php
```
Indsæt følgende PHP-kode:
```php
<?php
$servername = "database";  // Brug container-navnet
$username = "user";
$password = "password";
$dbname = "kundeDB";

// Opret forbindelse til databasen
$conn = new mysqli($servername, $username, $password, $dbname);

// Tjek forbindelsen
if ($conn->connect_error) {
    die("Forbindelsen mislykkedes: " . $conn->connect_error);
}

// Opret en tabel hvis den ikke eksisterer
$sql = "CREATE TABLE IF NOT EXISTS kunder (
    id INT AUTO_INCREMENT PRIMARY KEY,
    navn VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL
)";
$conn->query($sql);

// Indsæt testdata
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    $navn = $_POST["navn"];
    $email = $_POST["email"];
    
    $stmt = $conn->prepare("INSERT INTO kunder (navn, email) VALUES (?, ?)");
    $stmt->bind_param("ss", $navn, $email);
    $stmt->execute();
    echo "Ny kunde tilføjet!";
}

// Hent alle kunder
$result = $conn->query("SELECT * FROM kunder");

echo "<h2>Kunder:</h2>";
echo "<form method='POST'>
        Navn: <input type='text' name='navn' required>
        Email: <input type='text' name='email' required>
        <input type='submit' value='Tilføj kunde'>
      </form>";

echo "<ul>";
while ($row = $result->fetch_assoc()) {
    echo "<li>{$row['navn']} ({$row['email']})</li>";
}
echo "</ul>";

$conn->close();
?>
```

Dette script:
✔ Opretter en forbindelse til databasen  
✔ Opretter en tabel **`kunder`**, hvis den ikke findes  
✔ Lader brugeren indsætte en ny kunde via et **HTML-formular**  
✔ Viser alle kunder fra databasen  

---

### **5️⃣ Start Docker Compose**
Start hele systemet:
```sh
docker-compose up -d
```

---

### **6️⃣ Åbn Webapplikationen**
Gå til **`http://localhost:8080`**, og du vil se en formular, hvor du kan indsætte kunder i **kundeDB**.

---

### **7️⃣ Test at dataen gemmes**
1. Indsæt en testkunde via formularen.
2. Genstart containerne:
   ```sh
   docker-compose down
   docker-compose up -d
   ```
3. Gå til `http://localhost:8080` igen, og tjek at dataen stadig er der! 🎉

---

## **✅ Opsummering**
- **Vi har bygget et system med PHP + MySQL via Docker Compose**.
- **Data gemmes i en persistent MySQL-database (`kundeDB`)**.
- **Studerende lærer at forbinde en webapp med en database**.

Dette giver en **hands-on forståelse af container-baserede applikationer**! 🚀🔥

