#!/usr/bin/env bash
#
# Expose the stack over HTTPS with a Cloudflare *quick tunnel* — no domain, no inbound
# firewall ports. The tunnel dials OUT to Cloudflare and forwards to the frontend on :8081,
# where the SPA, /api, and Keycloak (/auth) are all served on one origin.
#
#   ./tunnel.sh up        Start the tunnel, bake its URL into the stack, deploy (HTTPS)
#   ./tunnel.sh down      Stop the stack and the tunnel
#   ./tunnel.sh url       Print the current public HTTPS URL
#   ./tunnel.sh logs      Follow the cloudflared log
#
# Why HTTPS at all: the SPA logs in with OIDC Authorization Code + PKCE, which needs
# window.crypto.subtle — only available in a *secure context* (HTTPS, or localhost). Over
# plain http://<public-ip> the login button silently does nothing. HTTPS fixes it for every
# browser with no flags.

set -euo pipefail
cd "$(dirname "$(readlink -f "$0")")"

BASE="docker-compose.yml"
OVERLAY="docker-compose.tunnel.yml"
CF_LOG=".cloudflared.log"
CF_PID=".cloudflared.pid"
TARGET="http://localhost:8081"

log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
err() { printf '\033[1;31mERROR:\033[0m %s\n' "$*" >&2; }

# --- compose command (v2 plugin preferred) ----------------------------------
if docker compose version >/dev/null 2>&1; then COMPOSE=(docker compose)
elif command -v docker-compose >/dev/null 2>&1; then COMPOSE=(docker-compose)
else err "Docker Compose not found."; exit 1; fi
COMPOSE+=(-f "$BASE" -f "$OVERLAY")

ensure_cloudflared() {
  if command -v cloudflared >/dev/null 2>&1; then return; fi
  err "cloudflared is not installed. Install it, then re-run:"
  cat >&2 <<'EOF'

  # Debian/Ubuntu (amd64):
  curl -fsSL https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
    -o /usr/local/bin/cloudflared && sudo chmod +x /usr/local/bin/cloudflared
  # (arm64: swap amd64 → arm64). Verify with:  cloudflared --version
EOF
  exit 1
}

ensure_env() { [[ -f .env ]] || { log ".env missing — creating from .env.example"; cp .env.example .env; }; }

# Upsert KEY=VALUE into .env (replace existing line or append).
set_env() {
  local key="$1" val="$2"
  if grep -q "^${key}=" .env 2>/dev/null; then
    sed -i "s|^${key}=.*|${key}=${val}|" .env
  else
    printf '%s=%s\n' "$key" "$val" >> .env
  fi
}

start_tunnel() {
  if [[ -f "$CF_PID" ]] && kill -0 "$(cat "$CF_PID")" 2>/dev/null; then
    log "cloudflared already running (pid $(cat "$CF_PID"))."
    return
  fi
  log "Starting Cloudflare quick tunnel → ${TARGET} ..."
  : > "$CF_LOG"
  nohup cloudflared tunnel --no-autoupdate --url "$TARGET" >>"$CF_LOG" 2>&1 &
  echo $! > "$CF_PID"
}

discover_url() {
  local url="" i
  for i in $(seq 1 30); do
    url=$(grep -oE 'https://[a-z0-9-]+\.trycloudflare\.com' "$CF_LOG" 2>/dev/null | head -1 || true)
    [[ -n "$url" ]] && { echo "$url"; return 0; }
    sleep 1
  done
  return 1
}

case "${1:-up}" in
  up)
    ensure_cloudflared
    ensure_env
    start_tunnel
    log "Waiting for the tunnel URL..."
    if ! URL=$(discover_url); then
      err "Could not read the tunnel URL from $CF_LOG. Last lines:"; tail -n 20 "$CF_LOG" >&2; exit 1
    fi
    log "Tunnel URL: $URL"
    set_env PUBLIC_ORIGIN "$URL"
    log "Building + starting the stack with the HTTPS overlay (first build is slow)..."
    "${COMPOSE[@]}" up -d --build
    "${COMPOSE[@]}" ps
    cat <<EOF

✅ Live over HTTPS:  $URL
   Log in there with superadmin/admin or chuyenvien/chuyenvien — works in any browser.

   The tunnel runs in the background (pid $(cat "$CF_PID")). Keep this server on to keep it up.
   Stop everything with:  ./tunnel.sh down
EOF
    ;;

  down)
    log "Stopping the stack..."
    "${COMPOSE[@]}" down --remove-orphans || true
    if [[ -f "$CF_PID" ]]; then
      log "Stopping cloudflared (pid $(cat "$CF_PID"))..."
      kill "$(cat "$CF_PID")" 2>/dev/null || true
      rm -f "$CF_PID"
    fi
    log "Done."
    ;;

  url)
    discover_url || { err "No tunnel URL found — is the tunnel running? (./tunnel.sh up)"; exit 1; }
    ;;

  logs)
    tail -f "$CF_LOG"
    ;;

  *)
    awk 'NR==1{next} /^#/{sub(/^# ?/,"");print;next} {exit}' "$0"; exit 1
    ;;
esac
