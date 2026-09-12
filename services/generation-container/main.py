"""
WanGP generation adapter.

A thin FastAPI service that wraps WanGP's Python API (shared/api.py).
The .NET VideoWorker calls POST /generate with local file paths that are
accessible to both containers via a shared Docker volume (/app/tmp).

Verified against C:/Users/kampe/Wan2GP/shared/api.py:

    init(*, output_dir=...) -> WanGPSession
        output_dir sets where WanGP writes generated files (session-level).
        There is no models_dir param — model paths come from wgp_config.json.

    session.submit_task(settings, callbacks=None) -> SessionJob
        No output_dir param. Settings dict is passed directly to WanGP.
        Reference image key: "image_start" (not "start_img").
        End frame key:       "image_end"   (not "end_img").
        video_length accepts an int (frames) or a string like "6s" (auto-converted).

    job.result(timeout=None) -> GenerationResult
        GenerationResult.generated_files: list[str]  — absolute paths on disk.
        GenerationResult.success: bool
        GenerationResult.errors: list[GenerationError]
"""

import asyncio
import logging
import os
import shutil
import sys
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

# WanGP location: /wangp in Docker, override with WANGP_PATH for local dev
_wangp_path = os.environ.get("WANGP_PATH", "/wangp")
sys.path.insert(0, _wangp_path)

logger = logging.getLogger("generation-adapter")
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(name)s — %(message)s",
)

try:
    from shared.api import init  # WanGP programmatic entry-point
    _wangp_available = True
except ImportError as exc:
    logger.warning("WanGP not importable (%s) — /generate will return 503", exc)
    _wangp_available = False

# WanGP writes all generated files into this directory.
# Must be on the shared volume so the .NET worker can read the output.
WANGP_OUTPUT_DIR = os.environ.get("WANGP_OUTPUT_DIR", "/app/tmp/wangp-out")

# Map clean model IDs → WanGP internal model_type strings
# Keys verified from Wan2GP/wgp.py and handler files.
MODEL_TYPE_MAP: dict[str, str] = {
    # ── Wan 2.1 ───────────────────────────────────────────────────────────────
    "wan-i2v":           "i2v",                      # i2v 14B 480p
    "wan-i2v-720p":      "i2v_720p",                 # i2v 14B 720p
    "wan-t2v":           "t2v",                      # t2v 14B
    "wan-t2v-1.3b":      "t2v_1.3B",                 # t2v 1.3B (fast)
    # ── MiniMax H3 ────────────────────────────────────────────────────────────
    "h3-fl2va":          "minimax_h3_fl2va_pruned",   # FL2VA Pruned 20B — PDD 8-step
    "h3-ref2va":         "minimax_h3_ref2va_pruned",  # Ref2VA Pruned 20B — PDD 8-step
    # ── LTX Video ─────────────────────────────────────────────────────────────
    "ltx-2":             "ltx2_19B",
    "ltx-2.3":           "ltx2_22B_distilled",
    "ltx-2.5":           "ltx2_25_22B",              # LTX 2.5 22B (distilled via LoRA)
}

# Per-model inference step counts.
# PDD / distilled models run well at 8 steps; non-distilled Wan 2.1 needs more.
MODEL_STEPS: dict[str, int] = {
    "i2v":                       20,
    "i2v_720p":                  20,
    "t2v":                       20,
    "t2v_1.3B":                  20,
    "minimax_h3_fl2va_pruned":   8,
    "minimax_h3_ref2va_pruned":  8,
    "ltx2_19B":                  8,
    "ltx2_22B_distilled":        8,
    "ltx2_25_22B":               8,
}

# One WanGP session; model stays in VRAM between requests
_session = None

# GPU is serialised — one generation at a time
_generation_lock = asyncio.Lock()


@asynccontextmanager
async def lifespan(app: FastAPI):
    global _session
    if _wangp_available:
        logger.info("Initialising WanGP session (output_dir=%s)…", WANGP_OUTPUT_DIR)
        os.makedirs(WANGP_OUTPUT_DIR, exist_ok=True)
        loop = asyncio.get_event_loop()
        # init() accepts output_dir (session-level); no models_dir param
        _session = await loop.run_in_executor(
            None,
            lambda: init(output_dir=WANGP_OUTPUT_DIR),
        )
        logger.info("WanGP session ready")
    else:
        logger.warning("WanGP unavailable — running without GPU session")
    yield
    logger.info("Adapter shutting down")


app = FastAPI(title="WanGP Generation Adapter", lifespan=lifespan)


# ── Request / response models ─────────────────────────────────────────────────

class GenerateRequest(BaseModel):
    prompt: str
    model: str = "ltx-2"
    width: int = 1280
    height: int = 720
    frame_count: int = 145       # frames; C# computes durationSeconds * 24 + 1
    reference_images: list[str] = []
    keyframes: list[str] = []
    reference_video: str | None = None  # local path for video-to-video conditioning
    output_path: str             # absolute path on the shared volume
    num_inference_steps: int | None = None  # overrides MODEL_STEPS default when set


class GenerateResponse(BaseModel):
    output_path: str


# ── Endpoints ─────────────────────────────────────────────────────────────────

@app.post("/generate", response_model=GenerateResponse)
async def generate(req: GenerateRequest):
    if _session is None:
        raise HTTPException(status_code=503, detail="WanGP session not ready")

    model_type = MODEL_TYPE_MAP.get(req.model, "ltx2_22B_distilled")

    settings: dict = {
        "model_type":          model_type,
        "prompt":              req.prompt,
        "resolution":          f"{req.width}x{req.height}",
        "video_length":        req.frame_count,   # int frames
        "num_inference_steps": req.num_inference_steps or MODEL_STEPS.get(model_type, 20),
    }

    # Verified key names from shared/api.py apply_media_flag_defaults():
    #   settings.get("image_start") → start frame flag "S"
    #   settings.get("image_end")   → end frame flag "E"
    if req.reference_images:
        settings["image_start"] = req.reference_images[0]

    if req.keyframes:
        settings["image_end"] = req.keyframes[-1]

    if req.reference_video:
        settings["video_start"] = req.reference_video
        logger.info("v2v conditioning: video_start=%s", req.reference_video)

    logger.info(
        "Generating: model=%s %dx%d %d frames — %r",
        model_type, req.width, req.height, req.frame_count,
        req.prompt[:80],
    )

    async with _generation_lock:
        try:
            loop = asyncio.get_event_loop()
            generated_path = await loop.run_in_executor(
                None,
                lambda: _run_generation(settings),
            )
        except Exception as exc:
            logger.error("WanGP generation failed: %s", exc, exc_info=True)
            raise HTTPException(status_code=500, detail=str(exc))

    # WanGP names the file itself (e.g. clip_001.mp4).
    # Move it to the exact path the .NET worker expects.
    os.makedirs(os.path.dirname(req.output_path), exist_ok=True)
    if generated_path != req.output_path:
        shutil.move(generated_path, req.output_path)

    logger.info("Generation complete → %s", req.output_path)
    return GenerateResponse(output_path=req.output_path)


@app.get("/health")
def health():
    return {
        "status":          "ready" if _session is not None else "initialising",
        "wangp_available": _wangp_available,
        "output_dir":      WANGP_OUTPUT_DIR,
    }


# ── Blocking helper (runs in thread pool) ────────────────────────────────────

def _run_generation(settings: dict) -> str:
    """
    Calls the WanGP Python API synchronously.
    Runs in a ThreadPoolExecutor so it doesn't block the asyncio event loop.

    submit_task(settings, callbacks=None) -> SessionJob   (no output_dir param)
    job.result() -> GenerationResult
    result.generated_files -> list[str]  (absolute paths written to WANGP_OUTPUT_DIR)
    """
    job    = _session.submit_task(settings)
    result = job.result()

    if not result.success:
        errors = "; ".join(str(e) for e in result.errors)
        raise RuntimeError(f"WanGP generation failed: {errors}")

    files = result.generated_files
    if not files:
        raise RuntimeError("WanGP returned no generated files")

    return files[0]
