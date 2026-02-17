# PrintMaster

Windows-Desktopanwendung (WPF, .NET 8), die Druckaufträge automatisch auslöst, sobald in überwachten Ordnern passende Dateien erscheinen.

## Anforderungen

- **.NET 8 SDK** (z. B. von [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0))
- Windows (WPF)

## Projektstruktur

```
PrintMaster/
├── Models/           # PrintWorkflow
├── Services/          # WorkflowStorage, FileWatcher, Print, PrinterDiscovery
├── ViewModels/        # MainViewModel, ViewModelBase, RelayCommand
├── Views/             # (MainWindow im Projektroot)
├── Styles/            # AppStyles.xaml
├── App.xaml(.cs)
├── MainWindow.xaml(.cs)
└── PrintMaster.csproj
```

## Funktionen

- **Workflows** definieren: Überwachter Ordner, Dateimuster (z. B. `Rechnung*.pdf`), Ziel-Drucker
- **Hintergrund-Überwachung** per `FileSystemWatcher`; bei neuer passender Datei wird automatisch gedruckt („printto“-Verb)
- **Konfiguration** wird in `%LocalAppData%\PrintMaster\workflows.json` gespeichert und beim Start geladen

## Anwendungsbeispiel: Versandetiketten Deutsche Post

Versandetiketten der Deutschen Post (z. B. aus dem Online-Frankiertool) sollen immer direkt auf den Etiketten-/Label-Drucker gehen, ohne manuelles Zuordnen.

**Beispieldateiname:**  
`Versandetiketten Deutsche Post A0060AXXXXX00000004C7.pdf`

**Workflow in PrintMaster:**

| Einstellung        | Wert                                                                 |
|--------------------|----------------------------------------------------------------------|
| **Name**           | z. B. „Deutsche Post Etiketten“                                     |
| **Überwachter Ordner** | Ordner, in den die PDFs gelegt werden (z. B. Downloads oder ein eigener Ordner) |
| **Dateimuster**    | `Versandetiketten Deutsche Post*.pdf` (Wildcard) oder Regex z. B. `^Versandetiketten Deutsche Post A\d+.*\.pdf$` |
| **Ziel-Drucker**   | Ihr Etiketten-/Label-Drucker (z. B. „Label-Drucker“)                 |

Sobald eine neue Datei mit diesem Muster im Ordner erscheint, wird sie automatisch an den gewählten Drucker gesendet. Optional kann im Workflow-Schritt 4 z. B. „Datei verschieben“ oder „Datei löschen“ nach dem Druck eingestellt werden.

## Build & Start

```bash
dotnet restore
dotnet build
dotnet run
```

## Hinweis

Zum Drucken wird `Process.Start` mit dem Verb `printto` verwendet. Funktioniert mit Dateitypen, die Windows standardmäßig drucken kann (z. B. PDF, wenn ein PDF-Drucktreiber/Viewer registriert ist). Fehler (fehlender Drucker, gesperrte Datei) werden abgefangen und führen nicht zum Absturz der App.
