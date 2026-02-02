FROM python:3.11-slim

# Install Tesseract + dependencies
RUN apt-get update && \
    apt-get install -y tesseract-ocr libtesseract-dev build-essential && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy FastAPI app
COPY main.py .

# Install Python dependencies including python-multipart
RUN pip install --no-cache-dir fastapi uvicorn pillow python-multipart

# Create temp OCR folder
RUN mkdir -p temp_ocr

EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
