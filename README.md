# AutoDocPrinter

AutoDocPrinter watches folders and automatically sends matching files to a printer.  
Great for shipping labels, online invoices, and other recurring documents.

---

## ✨ Highlights

- **Workflows** with:
  - A watched folder (e.g. Downloads)
  - One or more **file patterns** (wildcards or optional regex)
  - Target printer
  - Optional post‑processing (delete, move, rename)
- **Tray app**:
  - Minimizes to the system tray
  - Tray menu: open window, settings, log, exit
  - Balloon notifications on print errors
- **Language support**: German + English  
  (Automatically uses the system language; falls back to English)
- **Autostart option** via the settings
- **Backup & restore** for configuration

---

## 🧩 Workflows & file patterns

- You can create as many workflows as you like.
- Per workflow:
  - One watched folder
  - **Multiple file patterns**, e.g.:

    ```text
    AMZ*Invoice.pdf
    EBY*Invoice.pdf
    ETY*Invoice.pdf
    ```

    (for files like `AMZ1234_Shop_Invoice.pdf`, `EBY5678_Shop_Invoice.pdf`, …)

- **Pattern assistant**:
  - In the workflow editor you can select one or more example files.
  - The assistant suggests a wildcard pattern, e.g.:

    ```text
    AMZ1234_Shop_Invoice.pdf
    AMZ5678_Shop_Invoice.pdf
    → AMZ*Invoice.pdf
    ```

  - The suggestion is written into the **currently focused** pattern box.

- A workflow can have multiple pattern boxes:
  - „+“ adds another box
  - From the second box onwards each box has an **“X”** to remove it.

---

## 📝 Log & error handling

- Built‑in **log viewer**:
  - Time, workflow, file, success/failure, message
- On print errors:
  - Entry in the log
  - Tray balloon with workflow name + file name

---

## 💾 Backup & restore

- In **Settings**:
  - **“Create backup”**:
    - saves all relevant JSON configuration files  
      (`workflows.json`, `settings.json`, `language.json`, …) into a ZIP
  - **“Restore backup”**:
    - restores a previously created backup
    - shows a hint to restart the app afterwards

---

## ⚙️ Installation / usage

- This repository ships a **self‑contained single‑file EXE** (published in releases):
  - `AutoDocPrinter.exe` (win‑x64, no separate .NET install required)
- Just:
  1. Download the EXE from the release page
  2. Put it in a folder of your choice
  3. Run it – done

Optional:
- Enable autostart in the settings to start AutoDocPrinter automatically with Windows.

---

## 🔍 Known limitations

- Currently only the **“folder → printer”** scenario is supported  
  (no real Windows printer driver yet).
- Pattern assistant generates wildcards for “normal” names; exotic file names may still need manual tweaking.
- Dark mode is prepared but currently disabled – the UI uses the stable light theme.
