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

---

*Diese Liste kann laufend ergänzt werden.*
