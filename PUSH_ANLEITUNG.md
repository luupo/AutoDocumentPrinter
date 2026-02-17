# Repo zu GitHub/GitLab pushen

Das lokale Git-Repo ist angelegt, der erste Commit ist gemacht (Branch: `main`).

## Option A: Neues Repo auf GitHub erstellen

1. Auf [github.com](https://github.com) einloggen → **New repository**
2. Name z. B. **PrintMaster**, **Public**, **ohne** README/ .gitignore anlegen (Repo leer lassen)
3. Remote hinzufügen und pushen:

```bash
cd "d:\Users\Luca\Documents\Curser\PrintMaster"
git remote add origin https://github.com/DEIN_USERNAME/PrintMaster.git
git push -u origin main
```

(Für SSH: `git@github.com:DEIN_USERNAME/PrintMaster.git`)

## Option B: Neues Repo auf GitLab erstellen

1. Auf [gitlab.com](https://gitlab.com) → **New project** → **Create blank project**
2. Projektname z. B. **PrintMaster**
3. Dann:

```bash
cd "d:\Users\Luca\Documents\Curser\PrintMaster"
git remote add origin https://gitlab.com/DEIN_USERNAME/PrintMaster.git
git push -u origin main
```

---

Danach kannst du diese Datei (`PUSH_ANLEITUNG.md`) löschen oder im Repo lassen.
