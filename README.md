# Handwritten Diary OCR

- **Prerequisites:** Windows, .NET SDK, Docker  
- **Clone repo:** `git clone https://github.com/<yourusername>/handwritten-diary.git`  
- **Build OCR container:** `docker compose build`  
- **Start OCR container:** `docker compose up -d`  
- **Run ASP.NET app:** `cd FakeStrava` → `dotnet run`  
- **Open browser:** `http://localhost:5000/Ocr` → upload image → see OCR text  
- **Notes:** `ocr-data/` stores images/output, ignored by Git for privacy

------------------------------------------------------------------------------

# Handwritten Diary – Full Project Readme

## Part 1: Handwritten OCR Docker Setup

### Project Structure
- Project root: `handwritten-diary`
- Tesseract OCR folder: `tesseract`
- Data folder: `ocr-data` (host folder mapped to container `/data`)

### Step 1: Create DockerFile

1. Navigate to root folder.
2. Open PowerShell.
3. Create Tesseract folder and DockerFile:
   ```powershell
   mkdir tesseract
   cd tesseract
   ni DockerFile
   ```
4. Open DockerFile:
   ```powershell
   notepad DockerFile
   ```
5. Enter:
```
FROM ubuntu:22.04

RUN apt-get update && \
    apt-get install -y tesseract-ocr tesseract-ocr-eng && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /data
```

**Notes:**
- Base image Ubuntu 22.04
- Install Tesseract + English language
- Clean cache
- Set working dir `/data`

### Step 2: Docker Compose Setup

1. Go back to root:
   ```powershell
   cd ..
   ```
2. Create docker-compose.yml:
   ```powershell
   notepad docker-compose.yml
   ```
3. Enter:
```yaml
services:
  tesseract:
    build: ./tesseract
    container_name: tesseract
    volumes:
      - ./ocr-data:/data
    network_mode: none
    command: tail -f /dev/null
```

**Notes / Mental Model:**
- `docker-compose.yml` tells Docker what service to run, where files live, and allowed actions.
- `volumes` maps host `ocr-data` ↔ container `/data`
- `command: tail -f /dev/null` keeps container alive
- `network_mode: none` disables internet

**Mental model:** Tesseract is a sealed box. Slide images in → get text out.

### Step 3: Validate, Build, Run

1. Validate YAML with [YAML Lint](https://www.yamllint.com/)
2. Build:
   ```powershell
   docker compose build
   ```
   - Docker reads service, builds image from DockerFile
   - Caches layers for efficiency
3. Start container:
   ```powershell
   docker compose up -d
   ```
   - Mounts `ocr-data` → `/data`
   - Runs `tail -f /dev/null`
4. Check containers:
   ```powershell
   docker ps -a
   ```

**Mental model:**
- Build = assemble factory blueprint + stock parts
- Up / Run = start machine, ready for processing

### Step 4: Run Tesseract OCR

```powershell
docker exec tesseract tesseract input.png output --oem 1 --psm 6
```
- First `tesseract` = container name
- Second `tesseract` = OCR app inside container
- `input.png` → input image in `/data`
- `output` → output text file
- `--oem 1` → LSTM engine for handwriting
- `--psm 6` → single block of text

**Mental model:** Images go in, text comes out via shared folder.

---

## Part 2: ASP.NET Core MVC OCR App

### Goal (v1)
- Runs locally
- Single page web app
- Upload image → save to `ocr-data`
- Trigger OCR container
- Display extracted text
- No DB, LLM, or n8n yet

### Step 1: Create Project

```powershell
dotnet new mvc -n FakeStrava
```
- Creates folder `FakeStrava`
- Default MVC structure: Controllers, Views, wwwroot, Program.cs

**Mental model:** MVC app = thin UI + controller layer, moves files in/out of `ocr-data`

### Step 2: First Run

```powershell
cd FakeStrava
dotnet run
```
- Output: `Now listening on: http://localhost:5131`
- Confirm webpage loads → sanity check passed
- Stop app with `CTRL + C`

### Step 3: MVC Refresher

- Model → data + logic (OCR text, diary entries)
- View → what user sees (HTML, forms)
- Controller → handles requests (upload → run OCR → return result)

**Flow:** User → URL → Controller → Model → View

### Step 4: Controllers

- HomeController → landing page
- OcrController → OCR functionality
- DiaryController → runs & notes
- SummaryController → LLM summaries (later)

**URL mapping:** `/Ocr` → OcrController → Views/Ocr/Index.cshtml

### Step 5: OCR Integration Notes

- Keep Tesseract container running before starting MVC app
- Upload any image from laptop → copies to `ocr-data`
- `output.txt` gets overwritten with new OCR text
- `/data` in container maps to `ocr-data` on host → works seamlessly

### Step 6: Next Steps

- Create `OcrController` with upload action
- Create `Views/Ocr/Index.cshtml`
- Copy uploaded file → `ocr-data` → run OCR via `docker exec`
- Future: containerize MVC app for portability

