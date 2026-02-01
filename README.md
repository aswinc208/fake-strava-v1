
# FakeStrava v1 — Architecture & Setup Notes
# ❌Do not use - docker in docker issue not fixed yet.
## 1. Repository Setup

* Cloned the original repository:

  ```powershell
  git clone <repo-url> handwritten-diary
  ```
* Renamed the folder:

  ```powershell
  ren "handwritten-diary" "fake-strava-v1"
  ```

## 2. Initial Docker Compose Changes

* Edited `docker-compose.yml` to add a new microservice:

  * `fakestrava` → containerized ASP.NET MVC app
* App exposes container port `80`, mapped to a host port:

  * Example: `5000:80`
  * Format: `host_port:container_port`
* Notes on ports:

  * `container_port` is fixed at `80`

    * ASP.NET containers use Kestrel, which listens on port `80` by default (`EXPOSE 80`)
    * This cannot be changed without modifying the container runtime
  * `host_port` can be any unused port (`5000`, `8080`, `9000`, etc.)
* Inside Docker’s network, the app is accessible at:

  ```
  http://localhost:80
  ```

## 3. depends_on Configuration

* Added `depends_on` so the ASP.NET container waits for the OCR container to start:

  ```yaml
  depends_on:
    - ocr
  ```

## 4. Shared Volume (Initial Design)

* Shared volume mapping was added so both containers could access the same files:

  ```yaml
  volumes:
    - ./ocr-data:/app/ocr-data
  ```
* This allowed:

  * Image uploads from ASP.NET
  * OCR output written by Tesseract
* Inside the ASP.NET container, file paths were:

  ```csharp
  var ocrDir = "/app/ocr-data";
  var inputPath = Path.Combine(ocrDir, "input.png");
  var outputPath = Path.Combine(ocrDir, "output.txt");
  ```

## 5. Dockerfile for FakeStrava (ASP.NET MVC)

* Docker requires a `Dockerfile` inside the `FakeStrava/` folder because that is the build context referenced by `docker-compose.yml`
* Dockerfile structure:

  * **Stage 1 (base)** → runtime only, lightweight
  * **Stage 2 (build)** → full SDK, restore, build, publish
  * **Stage 3 (final)** → runtime only, copy published output
* `ENTRYPOINT` starts the ASP.NET app

## 6. .NET Version Mismatch Issue

* While running:

  ```bash
  docker compose build
  ```

  an error occurred:

  * `FakeStrava.csproj` was targeting `.NET 9.0`
  * Dockerfile was using `.NET 7.0` SDK and runtime images
* Fix options:

  * **Option A (chosen):**

    * Update project file:

      ```xml
      <TargetFramework>net7.0</TargetFramework>
      ```
  * **Option B (alternative):**

    * Upgrade Docker images:

      ```dockerfile
      FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
      FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
      ```
* Additional fix:

  * Removed `.NET 9`-only extensions from `Program.cs`:

    * `MapStaticAssets`
    * `WithStaticAssets`
* After these changes, the build completed successfully.

## 7. Docker Container Name Conflict

* While running:

  ```bash
  docker compose up -d
  ```

  Docker reported a container named `tesseract` was already running.
* Resolution steps:

  ```bash
  docker ps -a
  docker stop <container_id>
  docker rm tesseract
  docker ps -a
  ```
* After cleanup:

  * Both containers started successfully
  * `tesseract` container → OCR service, reads/writes `ocr-data/`
  * `fakestrava` container → ASP.NET MVC app, exposed on port `5000`

## 8. Runtime Failure During OCR Execution

* Web UI was accessible at:

  ```
  http://localhost:5000/Ocr
  ```
* Image upload succeeded
* Clicking **Run OCR** failed
* Root cause:

  * In the original setup:

    * ASP.NET ran on the **host**
    * Calling `docker exec tesseract ...` worked because Docker was installed on the host
  * In the new setup:

    * ASP.NET runs **inside a container**
    * Tesseract runs in **another container**
    * ASP.NET container does **not** have Docker installed
    * `Process.Start("docker", ...)` fails
* Installing Tesseract inside the ASP.NET container would work, but is an architectural anti-pattern.

## 9. Architecture Reset (Mental Refresh)

### Final High-Level Architecture

```
Browser
  ↓
FakeStrava (ASP.NET MVC)
  ↓ HTTP
OCR Service (FastAPI + Tesseract)
  ↓
Text result
```

### What Was Removed

* ASP.NET calling `docker exec`
* ASP.NET knowing anything about Docker
* Tight coupling between services
* Shared filesystem dependency (`ocr-data` can be removed)

### What Was Added

* Dedicated OCR microservice exposed via HTTP

## 10. OCR Service Responsibilities (FastAPI)

The OCR service will:

* Accept image upload (`POST /ocr`)
* Save image to a temporary file
* Run Tesseract locally inside its container
* Return extracted text as JSON

One job. One responsibility.

## 11. FakeStrava Responsibilities

FakeStrava will:

* Accept file upload from the browser
* Forward the file to the OCR service via HTTP
* Receive extracted text
* Render OCR output in the MVC view

FakeStrava never runs OCR directly.

## 12. Updated Project Structure

```
fake-strava-v1/
 ├─ FakeStrava/          # ASP.NET MVC app
 ├─ ocr-service/         # FastAPI + Tesseract
 │   ├─ main.py
 │   └─ Dockerfile
 ├─ docker-compose.yml
 └─ ocr-data/            # Legacy (can be removed)
```

## 13. Reference Commit

* Clean checkpoint before the architectural redesign:

  ```
  01d79eae76c5442f82e828ab48c3cf1f076b6fc2
  ```

---



