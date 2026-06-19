#!/usr/bin/env bash
#
# Deploy / undeploy the Industry & Trade web application with Docker Compose.
#
#   ./deploy.sh deploy [service...]     Build images and start the stack (detached)
#   ./deploy.sh undeploy [--purge]      Stop and remove containers (+ volumes with --purge)
#   ./deploy.sh redeploy [service...]   Undeploy then deploy (rebuild from scratch)
#   ./deploy.sh status                  Show container status
#   ./deploy.sh logs [service...]       Follow logs (all services or the named ones)
#   ./deploy.sh restart [service...]    Restart services
#
# --purge also deletes the named volumes (Postgres + MinIO data) — irreversible.

set -euo pipefail

# Run from the directory holding this script (the deploy/ folder) so the compose
# file and .env resolve regardless of where the script is invoked from.
cd "$(dirname "$(readlink -f "$0")")"

PROJECT="industrytrade"          # stable project name → deploy/undeploy match
COMPOSE_FILE="docker-compose.yml"

# --- Resolve the compose command (v2 plugin preferred, fall back to v1) ------
if docker compose version >/dev/null 2>&1; then
  COMPOSE=(docker compose)
elif command -v docker-compose >/dev/null 2>&1; then
  COMPOSE=(docker-compose)
else
  echo "ERROR: Docker Compose not found. Install Docker (with the compose plugin)." >&2
  exit 1
fi
COMPOSE+=(-p "$PROJECT" -f "$COMPOSE_FILE")

log() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }

ensure_env() {
  if [[ ! -f .env ]]; then
    log ".env not found — creating it from .env.example (adjust secrets for production!)"
    cp .env.example .env
  fi
}

print_urls() {
  cat <<'EOF'

Stack is up. Endpoints:
  • Frontend (web app) : http://localhost:8081
  • API + Swagger      : http://localhost:8080/swagger
  • Keycloak           : http://localhost:8090
  • MinIO console      : http://localhost:9001
  • RabbitMQ console    : http://localhost:15672

Demo logins: superadmin/admin (sees all) · chuyenvien/chuyenvien (scoped).
EOF
}

cmd="${1:-}"
[[ $# -gt 0 ]] && shift || true

case "$cmd" in
  deploy)
    ensure_env
    log "Building images and starting the stack..."
    "${COMPOSE[@]}" up -d --build "$@"
    "${COMPOSE[@]}" ps
    print_urls
    ;;

  undeploy)
    purge=false
    [[ "${1:-}" == "--purge" || "${1:-}" == "-v" ]] && purge=true
    if $purge; then
      log "Stopping the stack and DELETING volumes (Postgres + MinIO data)..."
      "${COMPOSE[@]}" down --volumes --remove-orphans
    else
      log "Stopping and removing containers (volumes/data kept)..."
      "${COMPOSE[@]}" down --remove-orphans
    fi
    log "Done."
    ;;

  redeploy)
    log "Redeploying (down, then build + up)..."
    "${COMPOSE[@]}" down --remove-orphans
    ensure_env
    "${COMPOSE[@]}" up -d --build "$@"
    "${COMPOSE[@]}" ps
    print_urls
    ;;

  status|ps)
    "${COMPOSE[@]}" ps
    ;;

  logs)
    "${COMPOSE[@]}" logs -f --tail=200 "$@"
    ;;

  restart)
    "${COMPOSE[@]}" restart "$@"
    ;;

  *)
    # Print the header comment block (lines after the shebang up to the first blank/code line).
    awk 'NR==1 { next } /^#/ { sub(/^# ?/, ""); print; next } { exit }' "$0"
    exit 1
    ;;
esac
