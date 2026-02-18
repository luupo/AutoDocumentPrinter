# PrintMaster – TODO / geplante Funktionen

*(Sortiert nach Priorität)*

---

## Priorität 1 (hoch)

### Eigener „PrintMaster“-Drucker
- **Virtueller Drucker:** Über die Einstellungen einen eigenen Drucker installierbar (z. B. „PrintMaster“), der im System wie ein normaler Drucker erscheint.
- **Neuer Workflow-Trigger:** Statt nur „Datei in Ordner“ → Alternative: **Druck über unseren Drucker**. Wenn jemand auf „PrintMaster“ druckt, löst das einen konfigurierten Workflow aus. Damit kann ohne Datei herunterladen direkt der Workflow getriggert werden (Druckauftrag → Ziel-Drucker/Nachverarbeitung).

---

## Priorität 2 (mittel)

### Export / Import
- **Workflows exportieren:** Alle oder ausgewählte Workflows als JSON-Datei speichern (z. B. für Weitergabe oder Umzug).
- **Workflows importieren:** Workflows aus einer JSON-Datei laden (optional: mit bestehenden zusammenführen oder ersetzen).

### Backup
- **Konfigurations-Backup:** Ein-Klick-Sicherung von `workflows.json` und `settings.json` (z. B. in einen Backup-Ordner mit Zeitstempel).
- Optional: automatisches Backup beim Start oder in festen Abständen.

---

## Priorität 3 / Backlog

- **Zeitfenster:** Workflows nur in bestimmten Uhrzeiten aktiv (z. B. nur 8–18 Uhr).
- **Max. Dateigröße:** Dateien über X MB nicht drucken oder separat behandeln.
- **Wiederholung bei Fehler:** Bei Druckfehler automatisch erneut versuchen (Anzahl/Intervall konfigurierbar).
- **Unterordner:** Überwachung auch in Unterordnern (rekursiv) optional pro Workflow.
- **Beim Start minimiert:** Option in den Einstellungen: App startet direkt in die Tray-Leiste.
- **Mehrere Dateimuster pro Workflow:** Liste von Patterns inkl. optionaler Ausschlussmuster.
- **Workflow-Prioritäten:** Reihenfolge/Ranking, falls mehrere Workflows auf dieselbe Datei passen.
- **Wartezeit bis Datei „stabil“ ist:** Zusatz-Option, dass die Datei X Sekunden unverändert sein muss, bevor gedruckt wird.
- **Konfliktbehandlung bei Ziel-Dateien:** Verhalten konfigurierbar (überschreiben, umbenennen, überspringen).
- **Wiederholversuche im Detail:** Konfigurierbare Anzahl/Wartezeit und Quarantäne-Ordner für dauerhaft fehlgeschlagene Drucke.
- **Quarantäne-Ordner:** Option, fehlerhafte Dateien in einen speziellen Fehler-Ordner zu verschieben.
- **Filter/Search im Log:** Nach Datum, Workflow, Erfolg/Fehler, Dateiname usw. filtern.
- **Log-Export:** Verlauf als CSV/JSON exportieren (z. B. zur Analyse oder für den Support).
- **Ruhiger Modus:** Benachrichtigungen nur bei Fehlern oder komplett stumm schalten.
- **Tray-Schnellaktionen:** Im Tray-Kontextmenü Workflows pausieren/fortsetzen oder „Letzten Fehler anzeigen“.
- **Setup-Assistent für neue Workflows:** Vordefinierte Vorlagen (z. B. Standard-PDF-Workflow für Ordner X).
- **Konfig-Schutz:** Option, Konfiguration per Passwort zu sperren (Nur-Lesen-Modus für produktive Installationen).
- **Konfigurations-Import/-Export mit Passwort:** Verschlüsselte JSON-Datei für die gesamte App-Konfiguration.

---

*Diese Liste kann laufend ergänzt werden.*
