# **Monster Trading Card Game (MTCG)**

## **📌 Projektbeschreibung**
Das **Monster Trading Card Game (MTCG)** ist ein serverbasiertes Sammelkartenspiel, das in C# entwickelt wurde. Spieler können Kartenpakete erwerben, Decks zusammenstellen, Kämpfe austragen und Karten mit anderen Spielern tauschen. Die Daten werden in einer PostgreSQL-Datenbank gespeichert.

---

## **🛠 Technologien & Tools**
- **Programmiersprache:** C# (.NET 6+)
- **Datenbank:** PostgreSQL
- **Server-Architektur:** Eigenentwickelter HTTP-Server
- **Containerisierung:** Docker
- **Testframework:** MSTest
- **Versionierung:** Git & GitHub

---

## **🚀 Installation & Setup**
### **1. Voraussetzungen**
- **.NET SDK 6+**
- **PostgreSQL** (lokal oder über Docker)
- **Visual Studio** oder eine andere C#-Entwicklungsumgebung
- **Docker** (falls Containerisierung genutzt wird)

### **2. Installation**
1. **Repository klonen**:
   ```sh
   git clone https://github.com/Anthonykev/Swen1_Monster_Trading_Cards_Game.git
   cd Swen1_Monster_Trading_Cards_Game
   ```

2. **Datenbank einrichten**:
   - Stelle sicher, dass PostgreSQL läuft und die Verbindung in `appsettings.json` korrekt konfiguriert ist.
   - Starte das Programm einmal, um die Datenbanktabellen automatisch zu erstellen.

3. **Server starten**:
   ```sh
   dotnet run
   ```

4. **Docker Container starten (optional):**
   ```sh
   docker-compose up --build
   ```

---

## **🔗 API-Endpunkte**
| Methode | Endpunkt | Beschreibung |
|---------|---------|--------------|
| `POST` | `/register` | Erstellt einen neuen Benutzer. |
| `POST` | `/login` | Authentifiziert einen Benutzer und gibt ein Token zurück. |
| `POST` | `/buy-package` | Ermöglicht den Kauf eines Kartenpakets. |
| `POST` | `/choose-deck` | Wählt vier Karten für das aktive Deck aus. |
| `POST` | `/battle-request` | Fordert einen Kampf an. |
| `GET` | `/get-elo-ranking` | Gibt die Rangliste basierend auf ELO zurück. |
| `POST` | `/set-motto` | Ändert das Motto des Benutzers. |

---

## **🎮 Spielmechanik**
- **Kartensystem:** Karten haben verschiedene Typen (`Monster`, `Spell`) und Elementtypen (`Fire`, `Water`, `Normal`).
- **Kampfmechanik:**
  - Kämpfe sind rundenbasiert.
  - Elementstärken/-schwächen beeinflussen den Schaden.
  - Spezialregeln für bestimmte Karten (z. B. `Kraken` ist immun gegen `Spells`).
- **Handelssystem:** Spieler können Karten tauschen, indem sie Angebote erstellen.

---

## **📂 Abgabedateien**
Die relevanten Abgabedateien befinden sich im Ordner `Abgabe`:
- `Monster_Trading_Cards_Backup.sql` → Datenbank-Schema (Tabellen ohne Daten)
- `MTCG_Protokoll_Anthony.pdf` → Projektbeschreibung und technische Details
- `ClassDiagram1.png` → Klassendiagramm des Systems

---

## **👥 Mitwirkende**
- **Projektleiter & Entwickler:** Anthonykev

Falls du das Projekt verbessern möchtest, erstelle bitte ein Issue oder einen Pull Request.

---

## **📜 Lizenz**
Dieses Projekt steht unter der **MIT-Lizenz**. Siehe [LICENSE](LICENSE) für weitere Details.

📌 **Offizielles Repository:** [GitHub - MTCG](https://github.com/Anthonykev/Swen1_Monster_Trading_Cards_Game)




