from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import os
import requests
import json
import logging

logging.basicConfig(level=logging.INFO, filename=r"D:\projects\TyreService\ocr-service\adapter.log",
    format="%(asctime)s %(message)s", filemode="w")
log = logging.getLogger("adapter")

app = FastAPI()


class OcrRequest(BaseModel):
    base64_image: str


UNLIMITED_OCR_URL = os.environ.get("UNLIMITED_OCR_URL", "http://127.0.0.1:10000")
HF_TOKEN = os.environ.get("HF_TOKEN", "")
MODEL_NAME = os.environ.get("UNLIMITED_OCR_MODEL", "Unlimited-OCR")
IMAGE_MODE = os.environ.get("UNLIMITED_OCR_IMAGE_MODE", "base")


def collect_stream_text(resp):
    chunks = []
    try:
        for raw_line in resp.iter_lines():
            if not raw_line:
                continue
            line = raw_line.decode("utf-8") if isinstance(raw_line, bytes) else raw_line
            if not line.startswith("data:"):
                continue
            data = line[len("data:"):].strip()
            if data == "[DONE]":
                break
            try:
                chunk = json.loads(data)
            except Exception:
                continue
            try:
                delta = chunk["choices"][0].get("delta", {})
                content = delta.get("content", "")
            except Exception:
                content = ""
            if content:
                chunks.append(content)
    except requests.RequestException:
        pass
    return "".join(chunks)


@app.post("/ocr")
def ocr_scan(req: OcrRequest):
    if not req.base64_image:
        raise HTTPException(status_code=400, detail="base64_image is required")

    # Build data URL (assume PNG if mime is unknown)
    data_url = f"data:image/png;base64,{req.base64_image}"

    payload = {
        "model": MODEL_NAME,
        "messages": [
            {"role": "user", "content": [{"type": "text", "text": "document parsing."}, {"type": "image_url", "image_url": {"url": data_url}}]}
        ],
        "temperature": 0,
        "skip_special_tokens": False,
        "images_config": {"image_mode": IMAGE_MODE},
    }

    url = UNLIMITED_OCR_URL.rstrip("/") + "/v1/chat/completions"
    log.info(f"Calling {url} (token={'set' if HF_TOKEN else 'MISSING'}, http_proxy={os.environ.get('HTTP_PROXY', 'none')}, https_proxy={os.environ.get('HTTPS_PROXY', 'none')})")
    headers = {"ngrok-skip-browser-warning": "true"}
    if HF_TOKEN:
        headers["Authorization"] = f"Bearer {HF_TOKEN}"
    # Bypass any system proxy for OCR requests
    os.environ.pop("HTTP_PROXY", None)
    os.environ.pop("HTTPS_PROXY", None)
    os.environ.pop("http_proxy", None)
    os.environ.pop("https_proxy", None)
    try:
        resp = requests.post(url, json=payload, timeout=1200, headers=headers)
    except requests.RequestException as e:
        log.error(f"Connection error: {e}")
        raise HTTPException(status_code=502, detail=f"Error contacting Unlimited-OCR: {e}")

    text = ""
    if resp.status_code == 200:
        # Try streaming collection
        text = collect_stream_text(resp)
        if not text:
            # Try non-streaming JSON response
            try:
                j = resp.json()
                # Try several possible locations for content
                if "choices" in j and len(j["choices"]) > 0:
                    choice = j["choices"][0]
                    if "message" in choice and isinstance(choice["message"], dict):
                        # Some servers return message.content as list
                        msg = choice["message"].get("content")
                        if isinstance(msg, list):
                            # concatenate text parts
                            parts = []
                            for p in msg:
                                if p.get("type") == "text":
                                    parts.append(p.get("text", ""))
                            text = "".join(parts)
                        elif isinstance(msg, str):
                            text = msg
                    elif "delta" in choice:
                        # fallback
                        text = choice["delta"].get("content", "")
            except Exception:
                text = ""
    else:
        body = resp.text[:1000]
        log.error(f"Unlimited-OCR returned {resp.status_code}: {body}")
        raise HTTPException(status_code=502, detail=f"Unlimited-OCR returned {resp.status_code}: {body[:200]}")

    if text:
        log.info(f"OCR text ({len(text)} chars): saved to ocr_debug.txt")
        with open(r"D:\projects\TyreService\ocr-service\ocr_debug.txt", "w", encoding="utf-8") as f:
            f.write(text)
    else:
        log.warning("OCR returned empty text")
    return {"text": text}
