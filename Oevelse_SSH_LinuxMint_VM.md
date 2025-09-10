
# Øvelse: Installation og brug af SSH mellem host og VM i Linux Mint

**Formål:**  
At installere og konfigurere SSH i en virtuel Linux Mint-maskine og oprette forbindelse fra hostmaskinen. Øvelsen giver studerende erfaring med klient-server kommunikation over SSH og træner dem i terminalarbejde og netværksforståelse.

---

## 🧩 Metadata

**Titel:** Installation og brug af SSH i Linux Mint VM  
**Fag:** Teknologi 1  
**Tema:** Fjernadgang, netværk, Linux  
**Niveau:** 2.  – Datamatiker  
**Forudsætninger:** Grundlæggende kendskab til terminal og netværk  
**Målgruppe:** Studerende der arbejder med Linux Mint i VirtualBox eller anden VM  
**Varighed:** 20–30 minutter

---

## 🎯 Læringsudbytte

Efter øvelsen kan den studerende:

1. Installere og aktivere SSH-server i Linux Mint
2. Kontrollere IP-adressen og netværksopsætningen i sin VM
3. Oprette forbindelse fra host til VM via SSH
4. Arbejde med en Linux-maskine via remote shell

---

## 🔧 Del 1: Installer SSH-server i din VM

1. Start din Linux Mint VM  
2. Åbn Terminal  
3. Kør følgende kommando:
```
sudo apt update
sudo apt install openssh-server
```

4. Start og tjek at SSH-serveren kører:
```
sudo systemctl start ssh
sudo systemctl status ssh
```

Du bør se:
```
Active: active (running)
```

---

## 🌐 Del 2: Find IP-adressen på din VM

Kør:
```
ip a
```

Find din IP-adresse, typisk under `enp0s3` eller `eth0`, fx:
```
inet 192.168.56.101
```

> OBS: Du skal have aktiveret **Bridged Adapter** eller **NAT med port forwarding** i din VM’s netværksindstillinger, ellers virker SSH ikke udefra.

---

## 🖥️ Del 3: SSH fra din hostmaskine

Fra din hostmaskine (Mac, Linux eller Windows med SSH):

```
ssh brugernavn@192.168.xx.xx
```

Eksempel:
```
ssh student@192.168.56.101
```

Hvis det er første gang, skal du acceptere forbindelsen og indtaste adgangskoden.

---

## 🧠 Øvelsesspørgsmål

- Hvad betyder det, at SSH bruger port 22?
- Hvordan kunne du sikre, at kun bestemte brugere har SSH-adgang?
- Hvad sker der, hvis SSH ikke kører, men porten er åben?
- Hvilke fordele giver SSH i forbindelse med DevOps og serveradministration?

---

## 📌 Bonusopgave

1. Prøv at logge ind via SSH og kør:
   ```
   top
   whoami
   hostname
   ```

2. Konfigurer din SSH-server til at skifte port fra 22 til fx 2222:
   - Rediger konfiguration:
     ```
     sudo nano /etc/ssh/sshd_config
     ```
   - Find linjen:
     ```
     #Port 22
     ```
     og ændr den til:
     ```
     Port 2222
     ```
   - Genstart SSH:
     ```
     sudo systemctl restart ssh
     ```

   - Forbind nu med:
     ```
     ssh -p 2222 student@192.168.56.101
     ```

---
