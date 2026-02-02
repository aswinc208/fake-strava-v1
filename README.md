Here’s a **very concise README.md** that focuses only on **setup and how it works**:

```markdown
# Fake Strava V1

A self-hosted diary app that converts handwritten notes to text using OCR. Fully local and privacy-first.

---

## How it Works

```

Browser → FakeStrava (ASP.NET MVC) → OCR Service (FastAPI + Tesseract) → JSON text

````

1. Upload a note in the web app  
2. OCR service converts it to text  
3. Text result returned and displayed on the page  

---

## Quick Setup

1. Clone repo:

```bash
git clone https://github.com/yourusername/fake-strava-v1.git
cd fake-strava-v1
````

2. Recreate OCR folder:

```powershell
mkdir ocr-data
New-Item -Path ocr-data\.gitkeep -ItemType File
```

3. Build Docker images:

```bash
docker compose build
```

4. Start containers:

```bash
docker compose up -d
```

5. Open in browser:

```
http://localhost:5000/Ocr/
```

6. Upload a handwritten note or receipt → see OCR JSON result

---

Everything runs locally; no data leaves your machine.

```

---

This is **short, to the point, and fully runnable**.  

If you want, I can also make an **even shorter “3-command Quick Start” version** for GitHub so someone can literally start the app in three lines. Do you want me to do that?
```
