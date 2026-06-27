import base64, io, os, time, json, re, threading, shutil
from typing import Any
from fastapi import FastAPI, Body
from fastapi.responses import StreamingResponse
import torch
from transformers import AutoModel, AutoTokenizer
from huggingface_hub import snapshot_download
from PIL import Image

import torch.multiprocessing as mp
try:
    mp.set_start_method("spawn", force=True)
except RuntimeError:
    pass
torch.set_num_threads(8)
torch.set_num_interop_threads(4)

import transformers.utils.import_utils
if not hasattr(transformers.utils.import_utils, 'is_torch_fx_available'):
    transformers.utils.import_utils.is_torch_fx_available = lambda: False

app = FastAPI()

MODEL_DIR = "/app/model"
model = None
tokenizer = None
loading = True

# Monkey-patch .cuda() to no-op on CPU (model's internal infer() calls .cuda() unconditionally)
if not torch.cuda.is_available():
    _orig_cuda = torch.Tensor.cuda
    def _cuda_noop(self, device=None, non_blocking=False, memory_format=torch.preserve_format):
        return self
    torch.Tensor.cuda = _cuda_noop

def load_model():
    global model, tokenizer, loading
    if not os.path.exists(MODEL_DIR):
        print(f"Downloading model baidu/Unlimited-OCR to {MODEL_DIR}...")
        os.makedirs(MODEL_DIR, exist_ok=True)
        snapshot_download("baidu/Unlimited-OCR", local_dir=MODEL_DIR)
        print("Download complete.")
    print(f"Loading model from {MODEL_DIR}...")
    start = time.time()
    os.makedirs("/tmp/ocr_results", exist_ok=True)
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR, trust_remote_code=True)
    model = AutoModel.from_pretrained(
        MODEL_DIR,
        trust_remote_code=True,
        use_safetensors=True,
        torch_dtype=torch.bfloat16,
        low_cpu_mem_usage=True,
    )
    model = model.eval()
    if torch.cuda.is_available():
        model = model.cuda()
    if tokenizer.eos_token_id is None:
        tokenizer.eos_token_id = 1
        tokenizer.pad_token_id = 1
    model.generation_config.eos_token_id = tokenizer.eos_token_id
    model.generation_config.pad_token_id = tokenizer.eos_token_id
    loading = False
    print(f"Model loaded in {time.time() - start:.0f}s")

def extract_image_text(messages):
    prompt = "document parsing."
    images = []
    for msg in messages:
        content = msg.get("content") if isinstance(msg, dict) else []
        if isinstance(content, str):
            content = json.loads(content) if content.startswith("[") else [{"type": "text", "text": content}]
        if isinstance(content, list):
            for part in content:
                if isinstance(part, dict):
                    if part.get("type") == "text":
                        prompt = part.get("text", prompt)
                    elif part.get("type") == "image_url":
                        url = part.get("image_url", {}).get("url", "")
                        if url.startswith("data:image"):
                            b64 = url.split(",", 1)[1].strip()
                            b64 += "=" * ((4 - len(b64) % 4) % 4)
                            images.append(base64.b64decode(b64, validate=False))
    return prompt, images

def images_to_pil(images_bytes):
    return [Image.open(io.BytesIO(img)) for img in images_bytes]

def clean_ocr_output(text):
    cleaned = re.sub(r"<\|det\|>.*?<\|/det\|>", "", text).strip()
    return cleaned

@app.get("/")
@app.get("/health")
def health():
    return {"status": "ok", "loading": loading}

@app.post("/v1/chat/completions")
def chat_completions(body: dict = Body(...)):
    while loading:
        time.sleep(1)
    messages = body.get("messages", [])
    images_config = body.get("images_config", {})
    stream = body.get("stream", False)
    prompt, images_bytes = extract_image_text(messages)
    if not images_bytes:
        return {"choices": [{"message": {"content": "Error: no valid image found in request"}, "index": 0}]}
    pil_images = images_to_pil(images_bytes)
    config = images_config
    base_size = 320
    image_size = 320 if config.get("image_mode") == "base" else 256
    crop_mode = config.get("image_mode") != "base"

    image_path = "/tmp/ocr_input.png"
    pil_images[0].save(image_path)

    t0 = time.time()
    with torch.no_grad():
        result = model.infer(
            tokenizer,
            prompt=f"<image>{prompt}",
            image_file=image_path or "",
            base_size=base_size, image_size=image_size, crop_mode=crop_mode,
            max_length=768,
            no_repeat_ngram_size=35, ngram_window=128,
            eval_mode=True, output_path="/tmp/ocr_results",
        )
    print(f"Inference took {time.time() - t0:.1f}s")

    text = clean_ocr_output(result if isinstance(result, str) else str(result))

    if stream:
        empty = json.dumps({"choices": [{"delta": {"content": ""}, "index": 0}]})
        done = json.dumps({"choices": [{"delta": {"content": text}, "index": 0}]})
        def sync_gen():
            yield "data: " + empty + "\n\n"
            yield "data: " + done + "\n\n"
            yield "data: [DONE]\n\n"
        return StreamingResponse(sync_gen(), media_type="text/event-stream")
    return {"choices": [{"message": {"content": text}, "index": 0}]}

threading.Thread(target=load_model, daemon=True).start()

if __name__ == "__main__":
    import uvicorn
    port = int(os.environ.get("PORT", 7860))
    uvicorn.run(app, host="0.0.0.0", port=port)
