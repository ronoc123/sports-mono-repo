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

# ── Paths (all under /workspace so they survive Pod restarts) ─────────────────
WORKSPACE="${WORKSPACE:-/workspace}"
REPO_DIR="${REPO_DIR:-$WORKSPACE/sports-mono-repo}"
VENV_DIR="$WORKSPACE/venv"
WANGP_DIR="$WORKSPACE/wangp"
MODELS_DIR="$WORKSPACE/models"
WORKER_BIN="$WORKSPACE/videoworker-bin"
DOTNET_DIR="$WORKSPACE/.dotnet"

# ── Config ────────────────────────────────────────────────────────────────────
WANGP_REPO="${WANGP_REPO:-https://github.com/deepbeepmeep/Wan2GP.git}"
WANGP_COMMIT="${WANGP_COMMIT:-main}"
WORKER_ID="${WORKER_ID:-worker-runpod-01}"
DOTNET_VERSION="8.0"

# ── Helpers ───────────────────────────────────────────────────────────────────
log()  { echo -e "\n\033[1;32m[setup]\033[0m $*"; }
err()  { echo -e "\n\033[1;31m[error]\033[0m $*" >&2; exit 1; }

log "RunPod GPU Worker Setup"
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
log "Checking GPU..."
nvidia-smi --query-gpu=name,memory.total --format=csv,noheader \
    || err "nvidia-smi failed — is this a GPU Pod?"
log "GPU OK"

# ── 2. System packages ────────────────────────────────────────────────────────
log "Installing system packages..."
apt-get update -qq
apt-get install -y -qq \
    curl git ffmpeg libgl1 libglib2.0-0 \
    python3-pip python3-venv \
    wget apt-transport-https

# ── 3. .NET 8 runtime (installed into /workspace/.dotnet to survive restarts) ─
log "Installing .NET $DOTNET_VERSION..."
if [[ ! -f "$DOTNET_DIR/dotnet" ]]; then
    mkdir -p "$DOTNET_DIR"
    # Microsoft's install script — installs into DOTNET_INSTALL_DIR
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
    # Use token for private repos if set
    if [[ -n "${GITHUB_TOKEN:-}" ]]; then
        git -C "$REPO_DIR" remote set-url origin \
            "$(git -C "$REPO_DIR" remote get-url origin | sed "s|https://|https://${GITHUB_TOKEN}@|")"
    fi
    git -C "$REPO_DIR" pull --ff-only
else
    : "${REPO_URL:?Set REPO_URL in RunPod env vars}"
    # Inject token into URL for private repos
    CLONE_URL="$REPO_URL"
    if [[ -n "${GITHUB_TOKEN:-}" ]]; then
        CLONE_URL="${REPO_URL/https:\/\//https:\/\/${GITHUB_TOKEN}@}"
    fi
    log "Cloning repo to $REPO_DIR..."
    git clone "$CLONE_URL" "$REPO_DIR"
fi

# ── 5. Python venv + WanGP dependencies ──────────────────────────────────────
log "Setting up Python venv..."
if [[ ! -d "$VENV_DIR" ]]; then
    python3 -m venv "$VENV_DIR"
fi
source "$VENV_DIR/bin/activate"

log "Installing PyTorch (CUDA 12.1)..."
pip install --quiet torch==2.3.1 torchvision==0.18.1 \
    --index-url https://download.pytorch.org/whl/cu121

# ── 6. Clone or update WanGP ─────────────────────────────────────────────────
log "Setting up WanGP..."
if [[ -d "$WANGP_DIR/.git" ]]; then
    log "WanGP exists — updating..."
    git -C "$WANGP_DIR" fetch
    git -C "$WANGP_DIR" checkout "$WANGP_COMMIT"
else
    log "Cloning WanGP ($WANGP_COMMIT)..."
    git clone "$WANGP_REPO" "$WANGP_DIR"
    git -C "$WANGP_DIR" checkout "$WANGP_COMMIT"
fi

log "Installing WanGP Python dependencies..."
pip install --quiet -r "$WANGP_DIR/requirements.txt" \
    --extra-index-url https://download.pytorch.org/whl/cu121

log "Installing FastAPI adapter dependencies..."
pip install --quiet -r "$REPO_DIR/services/generation-container/requirements.txt"

# ── 7. Publish VideoWorker ────────────────────────────────────────────────────
log "Publishing VideoWorker..."
dotnet publish "$REPO_DIR/services/VideoWorker" \
    -c Release \
    -o "$WORKER_BIN" \
    --self-contained false \
    -p:PublishSingleFile=false \
    --nologo \
    -v quiet
log "VideoWorker published to $WORKER_BIN"

# ── 8. Create model cache dir ─────────────────────────────────────────────────
mkdir -p "$MODELS_DIR"
mkdir -p "$WORKSPACE/tmp"

# ── 9. Write startup.sh ───────────────────────────────────────────────────────
log "Writing startup.sh..."
cat > "$WORKSPACE/startup.sh" <<STARTUP
#!/usr/bin/env bash
# startup.sh — Start FastAPI/WanGP + VideoWorker on RunPod
# Run this after vm-setup.sh:  bash /workspace/startup.sh

set -euo pipefail

WORKSPACE="${WORKSPACE}"
VENV_DIR="${VENV_DIR}"
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
echo "Starting FastAPI / WanGP adapter on localhost:8000 ..."
source "\$VENV_DIR/bin/activate"

# Point WanGP at the persistent model directory
export HF_HOME="\$MODELS_DIR"
export TORCH_HOME="\$MODELS_DIR"

cd "\$REPO_DIR/services/generation-container"
uvicorn main:app \\
    --host 127.0.0.1 \\
    --port 8000 \\
    --timeout-keep-alive 1800 \\
    --log-level info &

UVICORN_PID=\$!
echo "  FastAPI PID: \$UVICORN_PID"

# Wait for FastAPI to be ready before starting the worker
echo "Waiting for FastAPI to be ready..."
for i in \$(seq 1 30); do
    if curl -sf http://127.0.0.1:8000/health >/dev/null 2>&1; then
        echo "  FastAPI ready"
        break
    fi
    sleep 2
done

echo ""
echo "Starting VideoWorker..."
dotnet "\$WORKER_BIN/VideoWorker.dll" &
WORKER_PID=\$!
echo "  VideoWorker PID: \$WORKER_PID"

echo ""
echo "Both services running. Logs:"
echo "  FastAPI health:  curl http://127.0.0.1:8000/health"
echo "  Stop all:        kill \$UVICORN_PID \$WORKER_PID"
echo ""

# Keep script alive and exit if either process dies
wait -n \$UVICORN_PID \$WORKER_PID
echo "A service exited — check logs above"
STARTUP

chmod +x "$WORKSPACE/startup.sh"

# ── 10. Summary ───────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════╗"
echo "║               Setup Complete                                 ║"
echo "╠══════════════════════════════════════════════════════════════╣"
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
