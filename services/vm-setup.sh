#!/usr/bin/env bash
# vm-setup.sh — RunPod Pod setup (NO Docker / Docker Compose)
#
# A RunPod Pod is already a container — Docker Compose is not supported.
# This script installs dependencies directly into /workspace (which persists
# across Pod stop/start) and creates startup.sh to launch both services.
#
# Architecture on the Pod:
#   FastAPI / WanGP  →  localhost:8000
#   VideoWorker      →  polls MongoDB, calls localhost:8000, uploads to R2
#
# Runtime stack (defaults): Python 3.10.9 + PyTorch 2.7.1 + CUDA 12.8
#   Targets pods with CUDA 12.8 / driver 570 (e.g. RunPod RTX 4090).
#   WanGP requirements.txt Python version guard:
#     Python < 3.11  → onnxruntime-gpu 1.22.0  (cu128-compatible)  ← this path
#     Python >= 3.11 → onnxruntime-gpu 1.25.0.dev nightly  (cu130 feed)
#
# CUDA 13.0 upgrade (for pods with driver 575+):
#   Set these env vars before running:
#     PYTHON_VERSION=3.11.9
#     PYTORCH_VERSION=2.10.0
#     PYTORCH_TV_VERSION=0.25.0
#     PYTORCH_CUDA_STACK=cu130
#     CUDA_VERSION_EXPECTED=13.0
#
# ── How to use ────────────────────────────────────────────────────────────────
#
#  1. In RunPod, add these as Pod environment variables (or use RunPod Secrets
#     for the sensitive ones):
#
#       MONGODB_CONNECTION_STRING   (Secret)
#       R2_ACCESS_KEY               (Secret)
#       R2_SECRET_KEY               (Secret)
#       R2_ACCOUNT_ID
#       R2_BUCKET_NAME              (default: social-media-assets)
#       GITHUB_TOKEN                (Secret — only needed for private repo)
#       REPO_URL                    (your GitHub repo URL)
#       WANGP_COMMIT                (default: main)
#       WORKER_ID                   (default: worker-runpod-01)
#
#  2. SSH into the Pod and run:
#       bash /workspace/sports-mono-repo/services/vm-setup.sh
#
#     Or if the repo isn't cloned yet, bootstrap with:
#       export REPO_URL="https://github.com/your-username/sports-mono-repo.git"
#       export GITHUB_TOKEN="ghp_..."   # only for private repos
#       curl -fsSL "https://${GITHUB_TOKEN}@raw.githubusercontent.com/your-username/sports-mono-repo/main/services/vm-setup.sh" | bash
#
#  3. To start services:
#       bash /workspace/startup.sh

set -euo pipefail

# ── pip reliability ───────────────────────────────────────────────────────────
# Prevents download timeouts on slow RunPod egress and auto-retries transient
# failures.  Set before any pip call so they apply to the whole script.
export PIP_DEFAULT_TIMEOUT="${PIP_DEFAULT_TIMEOUT:-1000}"
export PIP_RETRIES="${PIP_RETRIES:-10}"

# ── Paths (all under /workspace so they survive Pod restarts) ─────────────────
WORKSPACE="${WORKSPACE:-/workspace}"
REPO_DIR="${REPO_DIR:-$WORKSPACE/sports-mono-repo}"
CONDA_DIR="$WORKSPACE/conda"          # Miniconda root
ENV_NAME="wangp"                      # conda environment name
WANGP_DIR="$WORKSPACE/wangp"
MODELS_DIR="$WORKSPACE/models"
WORKER_BIN="$WORKSPACE/videoworker-bin"
DOTNET_DIR="$WORKSPACE/.dotnet"

# ── Config ────────────────────────────────────────────────────────────────────
WANGP_REPO="${WANGP_REPO:-https://github.com/deepbeepmeep/Wan2GP.git}"
WANGP_COMMIT="${WANGP_COMMIT:-main}"
# Python / PyTorch stack — override these for CUDA 12.8 legacy pods:
#   PYTHON_VERSION=3.10.9 PYTORCH_VERSION=2.7.1 PYTORCH_TV_VERSION=0.22.1
#   PYTORCH_CUDA_STACK=cu128 CUDA_VERSION_EXPECTED=12.8
PYTHON_VERSION="${PYTHON_VERSION:-3.10.9}"
PYTORCH_VERSION="${PYTORCH_VERSION:-2.7.1}"
PYTORCH_TV_VERSION="${PYTORCH_TV_VERSION:-0.22.1}"
PYTORCH_CUDA_STACK="${PYTORCH_CUDA_STACK:-cu128}"
CUDA_VERSION_EXPECTED="${CUDA_VERSION_EXPECTED:-12.8}"

WORKER_ID="${WORKER_ID:-worker-runpod-01}"
DOTNET_VERSION="8.0"

# ── Helpers ───────────────────────────────────────────────────────────────────
log()  { echo -e "\n\033[1;32m[setup]\033[0m $*"; }
err()  { echo -e "\n\033[1;31m[error]\033[0m $*" >&2; exit 1; }

log "RunPod GPU Worker Setup"
echo "  Runtime:   Python $PYTHON_VERSION + PyTorch $PYTORCH_VERSION+$PYTORCH_CUDA_STACK (CUDA $CUDA_VERSION_EXPECTED)"
echo "  Services:  FastAPI/WanGP + .NET VideoWorker"
echo "  Storage:   /workspace (persists across Pod stop/start)"
echo ""

# ── Validate required env vars ────────────────────────────────────────────────
: "${MONGODB_CONNECTION_STRING:?Set MONGODB_CONNECTION_STRING in RunPod env vars or Secrets}"
: "${R2_ACCOUNT_ID:?Set R2_ACCOUNT_ID in RunPod env vars}"
: "${R2_ACCESS_KEY:?Set R2_ACCESS_KEY in RunPod Secrets}"
: "${R2_SECRET_KEY:?Set R2_SECRET_KEY in RunPod Secrets}"
R2_BUCKET_NAME="${R2_BUCKET_NAME:-social-media-assets}"

# ── 1. Verify GPU ─────────────────────────────────────────────────────────────
# Two-stage check: nvidia-smi proves the driver sees the card; cuInit(0) proves
# the CUDA runtime can actually open it.  nvidia-smi can succeed (driver OK)
# while cuInit returns 999 (CUDA_ERROR_NOT_READY / bad host mapping) — that
# combination wastes 45+ minutes of installation before failing at torch.
log "Checking GPU (nvidia-smi)..."
nvidia-smi --query-gpu=name,memory.total --format=csv,noheader \
    || err "nvidia-smi failed — is this a GPU Pod?"

log "Checking GPU (CUDA runtime — cuInit)..."
python3 - <<'CUDACHECK'
import ctypes, sys

try:
    cuda = ctypes.CDLL("libcuda.so.1")
except OSError as e:
    print(f"[FAIL] Cannot load libcuda.so.1: {e}", file=sys.stderr)
    sys.exit(1)

ret = cuda.cuInit(0)
if ret != 0:
    print(f"[FAIL] cuInit(0) returned {ret} — CUDA runtime is not usable on this host", file=sys.stderr)
    print("       Common cause: RunPod host mapping issue. Terminate this Pod and start a new one.", file=sys.stderr)
    sys.exit(1)

print("  cuInit(0) OK")
CUDACHECK

log "GPU OK"

# ── 2. System packages ────────────────────────────────────────────────────────
log "Installing system packages..."
apt-get update -qq
apt-get install -y -qq \
    curl git ffmpeg libgl1 libglib2.0-0

# ── 3. .NET 8 runtime (installed into /workspace/.dotnet to survive restarts) ─
log "Installing .NET $DOTNET_VERSION..."
if [[ ! -f "$DOTNET_DIR/dotnet" ]]; then
    mkdir -p "$DOTNET_DIR"
    curl -fsSL https://dot.net/v1/dotnet-install.sh \
        | bash -s -- --channel "$DOTNET_VERSION" --install-dir "$DOTNET_DIR"
    log ".NET installed to $DOTNET_DIR"
else
    log ".NET already installed — skipping"
fi
export DOTNET_ROOT="$DOTNET_DIR"
export PATH="$DOTNET_DIR:$PATH"
dotnet --version

# ── 4. Clone or update repo ───────────────────────────────────────────────────
log "Setting up repo..."
if [[ -d "$REPO_DIR/.git" ]]; then
    log "Repo exists — pulling latest..."
    if [[ -n "${GITHUB_TOKEN:-}" ]]; then
        git -C "$REPO_DIR" remote set-url origin \
            "$(git -C "$REPO_DIR" remote get-url origin | sed "s|https://|https://${GITHUB_TOKEN}@|")"
    fi
    git -C "$REPO_DIR" pull --ff-only
else
    : "${REPO_URL:?Set REPO_URL in RunPod env vars}"
    CLONE_URL="$REPO_URL"
    if [[ -n "${GITHUB_TOKEN:-}" ]]; then
        CLONE_URL="${REPO_URL/https:\/\//https:\/\/${GITHUB_TOKEN}@}"
    fi
    log "Cloning repo to $REPO_DIR..."
    git clone "$CLONE_URL" "$REPO_DIR"
fi

# ── 5. Miniconda + Python 3.10.9 environment ──────────────────────────────────
# We need an explicit Python 3.10 install rather than picking up the Pod's
# system Python (currently 3.12).  Miniconda persists under /workspace.
log "Setting up Miniconda..."
if [[ ! -f "$CONDA_DIR/bin/conda" ]]; then
    curl -fsSL https://repo.anaconda.com/miniconda/Miniconda3-latest-Linux-x86_64.sh \
        -o /tmp/miniconda.sh
    bash /tmp/miniconda.sh -b -p "$CONDA_DIR"
    rm -f /tmp/miniconda.sh
    log "Miniconda installed to $CONDA_DIR"
else
    log "Miniconda already installed — skipping"
fi

source "$CONDA_DIR/etc/profile.d/conda.sh"

log "Creating conda environment: $ENV_NAME (Python $PYTHON_VERSION)..."
if conda env list | grep -q "^$ENV_NAME "; then
    log "Conda env '$ENV_NAME' already exists — activating"
else
    conda create -y -n "$ENV_NAME" python="$PYTHON_VERSION"
    log "Created conda env: $ENV_NAME"
fi
conda activate "$ENV_NAME"

ACTIVE_PYTHON=$(python --version 2>&1)
log "Active Python: $ACTIVE_PYTHON"

# ── 6. Clone or update WanGP ─────────────────────────────────────────────────
log "Setting up WanGP (commit: $WANGP_COMMIT)..."
if [[ -d "$WANGP_DIR/.git" ]]; then
    log "WanGP exists — updating..."
    git -C "$WANGP_DIR" fetch
    git -C "$WANGP_DIR" checkout "$WANGP_COMMIT"
else
    log "Cloning WanGP..."
    git clone "$WANGP_REPO" "$WANGP_DIR"
    git -C "$WANGP_DIR" checkout "$WANGP_COMMIT"
fi

# ── 7. PyTorch — pinned BEFORE WanGP requirements ────────────────────────────
# Install torch first with --index-url so only the target CUDA index is used.
# WanGP's requirements.txt does not pin a torch version, so pip will see these
# as already satisfied and leave them alone.
log "Installing PyTorch $PYTORCH_VERSION ($PYTORCH_CUDA_STACK, pinned)..."
pip install --quiet \
    "torch==${PYTORCH_VERSION}" \
    "torchvision==${PYTORCH_TV_VERSION}" \
    "torchaudio==${PYTORCH_VERSION}" \
    --index-url "https://download.pytorch.org/whl/${PYTORCH_CUDA_STACK}"

log "Installing WanGP Python dependencies..."
# --extra-index-url (not --index-url) so PyPI is reachable for non-torch
# packages.  requirements.txt itself adds the CUDA 13 ONNX Runtime nightly
# feed via --extra-index-url at the top of the file; Python version guard
# then selects the appropriate onnxruntime-gpu build automatically.
pip install --quiet -r "$WANGP_DIR/requirements.txt" \
    --extra-index-url "https://download.pytorch.org/whl/${PYTORCH_CUDA_STACK}"

# ── 8. Verify PyTorch — hard gate, aborts setup on failure ───────────────────
log "Verifying PyTorch (must be ${PYTORCH_VERSION} + CUDA ${CUDA_VERSION_EXPECTED})..."
PYTORCH_VERSION_CHECK="$PYTORCH_VERSION" CUDA_VERSION_CHECK="$CUDA_VERSION_EXPECTED" \
python - <<'PYCHECK'
import os, sys
import torch

expected_torch = os.environ["PYTORCH_VERSION_CHECK"]
expected_cuda  = os.environ["CUDA_VERSION_CHECK"]

failures = []

if not torch.__version__.startswith(expected_torch):
    failures.append(f"torch version = {torch.__version__!r}  (expected {expected_torch}+)")

if torch.version.cuda != expected_cuda:
    failures.append(f"torch CUDA    = {torch.version.cuda!r}  (expected '{expected_cuda}')")

if not torch.cuda.is_available():
    failures.append("torch.cuda.is_available() returned False")

n = torch.cuda.device_count()
if n < 1:
    failures.append(f"torch.cuda.device_count() = {n}  (no GPU visible)")

if failures:
    print("\n[FAIL] PyTorch verification failed — setup aborted", file=sys.stderr)
    for f in failures:
        print(f"  {f}", file=sys.stderr)
    print(
        f"\nFix: ensure RunPod Pod driver supports CUDA {expected_cuda} and rerun vm-setup.sh",
        file=sys.stderr,
    )
    sys.exit(1)

print(f"  OK  torch {torch.__version__}  CUDA {torch.version.cuda}  GPUs: {n}")
PYCHECK

log "Installing FastAPI adapter dependencies..."
pip install --quiet -r "$REPO_DIR/services/generation-container/requirements.txt"

# ── 9. Publish VideoWorker ────────────────────────────────────────────────────
log "Publishing VideoWorker..."
dotnet publish "$REPO_DIR/services/VideoWorker" \
    -c Release \
    -o "$WORKER_BIN" \
    --self-contained false \
    -p:PublishSingleFile=false \
    --nologo \
    -v quiet
log "VideoWorker published to $WORKER_BIN"

# ── 10. Create model and temp dirs ────────────────────────────────────────────
mkdir -p "$MODELS_DIR"
mkdir -p "$WORKSPACE/tmp"

# ── 11. Write startup.sh ──────────────────────────────────────────────────────
log "Writing startup.sh..."
cat > "$WORKSPACE/startup.sh" <<STARTUP
#!/usr/bin/env bash
# startup.sh — Start FastAPI/WanGP + VideoWorker on RunPod
# Run this after vm-setup.sh:  bash /workspace/startup.sh

set -euo pipefail

WORKSPACE="${WORKSPACE}"
CONDA_DIR="${CONDA_DIR}"
ENV_NAME="${ENV_NAME}"
WANGP_DIR="${WANGP_DIR}"
MODELS_DIR="${MODELS_DIR}"
WORKER_BIN="${WORKER_BIN}"
DOTNET_DIR="${DOTNET_DIR}"
REPO_DIR="${REPO_DIR}"

export DOTNET_ROOT="\$DOTNET_DIR"
export PATH="\$DOTNET_DIR:\$PATH"
export WANGP_PATH="\$WANGP_DIR"
export WANGP_OUTPUT_DIR="\$WORKSPACE/tmp/wangp-out"

mkdir -p "\$WANGP_OUTPUT_DIR" "\$WORKSPACE/tmp"

echo ""
echo "Activating Python environment (\$ENV_NAME)..."
source "\$CONDA_DIR/etc/profile.d/conda.sh"
conda activate "\$ENV_NAME"

# Point WanGP at the persistent model directory
export HF_HOME="\$MODELS_DIR"
export TORCH_HOME="\$MODELS_DIR"

echo "Starting FastAPI / WanGP adapter on localhost:8000 ..."
cd "\$REPO_DIR/services/generation-container"
uvicorn main:app \\
    --host 127.0.0.1 \\
    --port 8000 \\
    --timeout-keep-alive 1800 \\
    --log-level info &

UVICORN_PID=\$!
echo "  FastAPI PID: \$UVICORN_PID"

# Wait for FastAPI to be ready before starting the worker.
# WanGP may load a large model on first start — allow up to 4 minutes.
# Also check that uvicorn is still alive on each iteration; if it died we
# fail immediately rather than waiting out the full timeout.
echo "Waiting for FastAPI to be ready (up to 4 min)..."
READY=false
for i in \$(seq 1 120); do
    if curl -sf http://127.0.0.1:8000/health >/dev/null 2>&1; then
        echo "  FastAPI ready (after \${i}x2 s)"
        READY=true
        break
    fi
    if ! kill -0 "\$UVICORN_PID" 2>/dev/null; then
        echo "[ERROR] FastAPI process exited during startup — check uvicorn logs above"
        exit 1
    fi
    sleep 2
done

if [[ "\$READY" != "true" ]]; then
    echo "[ERROR] FastAPI did not become ready within 4 minutes"
    kill "\$UVICORN_PID" 2>/dev/null || true
    exit 1
fi

echo ""
echo "Starting VideoWorker..."

# ASP.NET Core reads config from env vars where __ maps to the : separator.
# RunPod sets flat names (MONGODB_CONNECTION_STRING etc.) so we translate them
# here.  These are also needed because appsettings.json defaults VideoGenerator
# to Stub mode and points WanGP at the Docker hostname, not localhost.
export MongoDB__ConnectionString="\${MONGODB_CONNECTION_STRING}"
export CloudflareR2__AccountId="\${R2_ACCOUNT_ID}"
export CloudflareR2__AccessKey="\${R2_ACCESS_KEY}"
export CloudflareR2__SecretKey="\${R2_SECRET_KEY}"
export CloudflareR2__BucketName="\${R2_BUCKET_NAME:-social-media-assets}"
export VideoGenerator__Type="WanGp"
export VideoGenerator__WanGp__ApiUrl="http://127.0.0.1:8000"
export VideoGenerator__WanGp__TimeoutSeconds="7200"
export Worker__Id="\${WORKER_ID:-worker-runpod-01}"
export Worker__StaleJobTimeoutMinutes="120"

dotnet "\$WORKER_BIN/VideoWorker.dll" &
WORKER_PID=\$!
echo "  VideoWorker PID: \$WORKER_PID"

echo ""
echo "Both services running."
echo "  FastAPI health:  curl http://127.0.0.1:8000/health"
echo "  Stop all:        kill \$UVICORN_PID \$WORKER_PID"
echo ""

# Keep script alive and exit if either process dies
wait -n \$UVICORN_PID \$WORKER_PID
echo "A service exited — check logs above"
STARTUP

chmod +x "$WORKSPACE/startup.sh"

# ── 12. Summary ───────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════╗"
echo "║               Setup Complete                                 ║"
echo "╠══════════════════════════════════════════════════════════════╣"
printf "║  Runtime:  Python %-5s  +  PyTorch %-7s %-6s        ║\n" \
    "$PYTHON_VERSION" "$PYTORCH_VERSION" "$PYTORCH_CUDA_STACK"
printf "║  WanGP:    %-51s ║\n" "$WANGP_COMMIT"
echo "║                                                              ║"
echo "║  Start services:                                             ║"
echo "║    bash /workspace/startup.sh                                ║"
echo "║                                                              ║"
echo "║  First job will trigger model download (~10-20 GB).         ║"
echo "║  Watch progress:  curl http://127.0.0.1:8000/health          ║"
echo "║                                                              ║"
echo "║  After git push, update with:                                ║"
echo "║    git -C /workspace/sports-mono-repo pull                   ║"
echo "║    dotnet publish .../VideoWorker -c Release -o \$WORKER_BIN  ║"
echo "║    bash /workspace/startup.sh                                ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""
log "Run: bash /workspace/startup.sh"
