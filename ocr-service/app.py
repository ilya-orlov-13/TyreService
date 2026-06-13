import base64
import os
import numpy as np
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import cv2
import easyocr

app = FastAPI(title="EasyOCR Service")

reader = None


def get_reader():
    global reader
    if reader is None:
        reader = easyocr.Reader(
            ["ru", "en"],
            gpu=False,
            model_storage_directory=os.path.join(os.path.dirname(__file__), "models"),
        )
    return reader


class OcrRequest(BaseModel):
    base64_image: str


class OcrResponse(BaseModel):
    text: str


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/ocr", response_model=OcrResponse)
def ocr(request: OcrRequest):
    try:
        image_data = base64.b64decode(request.base64_image)
        nparr = np.frombuffer(image_data, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
        enhanced = clahe.apply(gray)
        r = get_reader()
        result = r.readtext(enhanced, paragraph=False, min_size=10)
        sorted_result = sorted(result, key=lambda x: (x[0][0][1], x[0][0][0]))
        text = "\n".join([item[1] for item in sorted_result])
        return OcrResponse(text=text)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
