# Generation container — WanGP / LTX 2.3 + FastAPI adapter
#
# Build args:
#   WANGP_REPO   — git URL for WanGP (default: deepbeepmeep/Wan2GP)
#   WANGP_COMMIT — branch, tag, or commit SHA to pin (default: main)
#
# Volumes expected at runtime:
#   /models  — model weight files; persisted across container restarts
#   /app/tmp — shared working directory with the VideoWorker container
#
# The model weights are NOT baked into the image.
# On first start, WanGP will download them from HuggingFace into /models.

ARG CUDA_VERSION=12.1.1
ARG WANGP_REPO=https://github.com/deepbeepmeep/Wan2GP.git
ARG WANGP_COMMIT=main

FROM nvidia/cuda:${CUDA_VERSION}-cudnn8-devel-ubuntu22.04

ENV DEBIAN_FRONTEND=noninteractive \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    WANGP_OUTPUT_DIR=/app/tmp/wangp-out

# System dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    python3.10 python3.10-dev python3-pip \
    git ffmpeg libgl1 libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/* \
    && ln -sf /usr/bin/python3.10 /usr/local/bin/python \
    && ln -sf /usr/bin/pip3       /usr/local/bin/pip

# ── Clone WanGP ───────────────────────────────────────────────────────────────
WORKDIR /wangp
ARG WANGP_REPO
ARG WANGP_COMMIT
RUN git clone ${WANGP_REPO} . && git checkout ${WANGP_COMMIT}

# ── Install PyTorch (pinned to CUDA 12.1) ────────────────────────────────────
RUN pip install torch==2.3.1 torchvision==0.18.1 \
    --index-url https://download.pytorch.org/whl/cu121

# ── Install WanGP's own dependencies ─────────────────────────────────────────
RUN pip install -r requirements.txt \
    --extra-index-url https://download.pytorch.org/whl/cu121

# ── Install adapter dependencies ──────────────────────────────────────────────
COPY requirements.txt /app/adapter-requirements.txt
RUN pip install -r /app/adapter-requirements.txt

# ── Copy the adapter ──────────────────────────────────────────────────────────
WORKDIR /app
COPY main.py .

# Model weights and shared working directory are mounted as volumes
VOLUME ["/models"]
VOLUME ["/app/tmp"]

EXPOSE 8000

# Long keep-alive matches the generation timeout (30 min)
CMD ["uvicorn", "main:app", \
     "--host", "0.0.0.0", \
     "--port", "8000", \
     "--timeout-keep-alive", "1800"]
