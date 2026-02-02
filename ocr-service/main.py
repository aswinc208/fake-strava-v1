from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
import shutil
import subprocess
import uuid
import os

app = FastAPI()

OCR_TEMP_DIR = "temp_ocr"
os.makedirs(OCR_TEMP_DIR, exist_ok=True)

@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/ocr")
async def ocr_image(file: UploadFile = File(...)):
    # Save uploaded file to temp folder
    file_ext = os.path.splitext(file.filename)[1]
    temp_filename = f"{uuid.uuid4()}{file_ext}"
    temp_path = os.path.join(OCR_TEMP_DIR, temp_filename)

    with open(temp_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    # Run Tesseract OCR
    try:
        subprocess.run(
            ["tesseract", temp_path, temp_path],
            check=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
        with open(temp_path + ".txt", "r", encoding="utf-8") as f:
            text = f.read()
    except subprocess.CalledProcessError as e:
        return JSONResponse(
            status_code=500, content={"error": e.stderr.decode()}
        )

    # Clean up input file
    os.remove(temp_path)

    return {"text": text}
