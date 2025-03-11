# Docker Networks – Fra Grundlæggende til Avanceret

## Metadata

- **Titel:** Docker Networks – Fra Grundlæggende til Avanceret
- **Målgruppe:** Studerende inden for IT, DevOps og netværksadministration
- **Faglige mål:**
  - Forstå forskellige Docker-netværkstyper
  - Praktisk erfaring med opsætning og brug af Docker-netværk
  - Forstå netværksisolering og kommunikation mellem containere
- **Forudsætninger:** Grundlæggende kendskab til Linux og Docker
- **Estimeret varighed:** 2 timer

---

## Trin 1: Forbered dit lab-miljø

1. Opret en Ubuntu VM i VirtualBox.
2. Installer Docker med følgende kommandoer:

```bash
sudo apt update
sudo apt install docker.io -y
```

## Opgave 1: Default Bridge Network

1. Kør en container med BusyBox:

```bash
sudo docker run -itd --name container1 busybox
```

2. Inspicer netværket:

```bash
sudo docker network inspect bridge
```

- Find IP-adressen og ping containeren fra Docker-hosten.

## Opgave 2: User-defined Bridge Network

1. Opret et brugerdefineret netværk:

```bash
sudo docker network create mitnetvaerk
```

2. Kør to containere i dette netværk:

```bash
sudo docker run -itd --name web1 --network mitnetvaerk nginx
sudo docker run -itd --name web2 --network mitnetvaerk nginx
```

3. Test DNS-forbindelsen ved at gå ind i `web1` og ping web2:

```bash
sudo docker exec -it web1 sh
ping web2
```

## Opgave 3: Host Network

1. Kør en Nginx-container i host-netværket:

```bash
sudo docker run -itd --name webhost --network host nginx
```

2. Tilgå containerens hjemmeside direkte fra hostens IP-adresse (fx `http://host-ip`).

## Opgave 4: MacVLAN Network

1. Opret et MacVLAN-netværk (tilpas IP til dit eget netværk):

```bash
sudo docker network create -d macvlan \
  --subnet=192.168.1.0/24 \
  --gateway=192.168.1.1 \
  -o parent=ens33 \
  macvlan-net
```

2. Kør en container med statisk IP:

```bash
sudo docker run -itd --name macvlan_container --network macvlan-net --ip=192.168.1.200 busybox
```

3. Bekræft at containeren kan pinge routeren og internettet:

```bash
sudo docker exec -it macvlan_container sh
ping 192.168.1.1
ping google.com
```

## Bonus Opgave (Avanceret): IP VLAN L3

1. Opret et IP VLAN L3-netværk:

```bash
sudo docker network create -d ipvlan \
  --subnet=192.168.100.0/24 \
  --subnet=192.168.101.0/24 \
  -o parent=ens33 \
  -o ipvlan_mode=l3 ipvlan-net
```

2. Kør containere med specifikke IP-adresser:

```bash
sudo docker run -itd --name thor --network ipvlan-net --ip 192.168.100.10 busybox
sudo docker run -itd --name loki --network ipvlan-net --ip 192.168.101.10 busybox
```

3. Test forbindelse mellem containerne:

```bash
sudo docker exec -it thor sh
ping 192.168.101.10
```

Forklar, hvordan du kunne få adgang til containerne fra dit lokale netværk vha. statiske ruter på hosten og din router.

---

**Aflevering:**

Screenshot dine kommandoer og outputs. Lav en kort opsummering af, hvad du har lært om hvert netværk.

