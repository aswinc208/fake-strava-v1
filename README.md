# Fake Strava V1

A self-hosted app that extracts strava data using OCR. Fully local and privacy-first.

---

1. Upload a note in the web app  
2. OCR service converts it to text  
3. Text result returned and displayed on the page  

---

## Quick Setup

1. Clone repo:
git clone https://github.com/yourusername/fake-strava-v1.git
cd fake-strava-v1

3. Build Docker images:
docker compose build

4. Start containers:
docker compose up -d

5. Open in browser:
http://localhost:5000/Ocr/

6. Upload strava screenshots → see OCR JSON result
Everything runs locally; no data leaves your machine.


