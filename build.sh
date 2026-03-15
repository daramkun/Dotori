#!/usr/bin/env bash
# build.sh — Dotori 전체 빌드 스크립트
# 각 컴포넌트를 빌드하여 build/<component>/ 폴더에 출력합니다.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="$REPO_ROOT/build"
CONFIG="${CONFIG:-Release}"
RID=""        # --rid  <runtime-id>  (예: linux-x64, win-x64, osx-arm64)
VERSION=""    # --version <ver>       (예: v1.2.3)

# 색상 출력
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; CYAN='\033[0;36m'; NC='\033[0m'
info()    { echo -e "${CYAN}[build]${NC} $*"; }
success() { echo -e "${GREEN}[ok]${NC} $*"; }
warn()    { echo -e "${YELLOW}[warn]${NC} $*"; }
error()   { echo -e "${RED}[error]${NC} $*" >&2; }

# 빌드 대상 목록 (--only 옵션으로 필터링 가능)
ALL_TARGETS=(language_server cli build_server worker registry )

# 옵션 파싱
ONLY_TARGETS=()
SKIP_TARGETS=()
SHOW_HELP=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --only)    shift; IFS=',' read -ra ONLY_TARGETS <<< "$1" ;;
        --skip)    shift; IFS=',' read -ra SKIP_TARGETS <<< "$1" ;;
        --config)  shift; CONFIG="$1" ;;
        --rid)     shift; RID="$1" ;;
        --version) shift; VERSION="$1" ;;
        --help|-h) SHOW_HELP=true ;;
        *) error "알 수 없는 옵션: $1"; SHOW_HELP=true ;;
    esac
    shift
done

if $SHOW_HELP; then
    cat <<EOF
사용법: ./build.sh [옵션]

옵션:
  --only <targets>   지정한 대상만 빌드 (쉼표 구분)
  --skip <targets>   지정한 대상은 건너뜀 (쉼표 구분)
  --config <cfg>     빌드 구성 (기본값: Release)
  --rid <rid>        .NET Runtime Identifier (예: linux-x64, win-x64, osx-arm64)
                     지정 시 --self-contained 으로 퍼블리시
  --version <ver>    어셈블리 버전 (예: v1.2.3) — -p:Version 으로 전달
  --help             이 도움말 출력

대상 목록: ${ALL_TARGETS[*]}

예시:
  ./build.sh
  ./build.sh --only cli,language_server
  ./build.sh --only cli,build_server,worker --rid linux-x64
  ./build.sh --only cli,build_server,worker --rid win-x64 --version v1.0.0
  ./build.sh --config Debug
EOF
    exit 0
fi

should_build() {
    local target="$1"
    if [[ ${#SKIP_TARGETS[@]} -gt 0 ]]; then
        for s in "${SKIP_TARGETS[@]}"; do [[ "$s" == "$target" ]] && return 1; done
    fi
    if [[ ${#ONLY_TARGETS[@]} -gt 0 ]]; then
        for o in "${ONLY_TARGETS[@]}"; do [[ "$o" == "$target" ]] && return 0; done
        return 1
    fi
    return 0
}

# ──────────────────────────────────────────────────────────────────────────────
# .NET 컴포넌트 빌드 헬퍼
# ──────────────────────────────────────────────────────────────────────────────
dotnet_publish() {
    local label="$1"
    local proj="$2"
    local out_dir="$3"
    local extra_args="${4:-}"

    # --rid 가 지정된 경우 self-contained 퍼블리시
    local rid_args=""
    if [[ -n "$RID" ]]; then
        rid_args="-r $RID --self-contained"
    fi

    # --version 이 지정된 경우 어셈블리 버전 주입
    local ver_args=""
    if [[ -n "$VERSION" ]]; then
        ver_args="-p:Version=${VERSION#v}"   # 'v' 접두사 제거 (v1.2.3 → 1.2.3)
    fi

    info "[$label] 빌드 중...${RID:+ (rid=$RID)}${VERSION:+ (ver=$VERSION)} → $out_dir"
    mkdir -p "$out_dir"
    # shellcheck disable=SC2086
    dotnet publish "$proj" \
        -c "$CONFIG" \
        -o "$out_dir" \
        --nologo \
        -v quiet \
        $rid_args \
        $ver_args \
        $extra_args
    success "[$label] 완료"
}

# ──────────────────────────────────────────────────────────────────────────────
# 1. CLI (NativeAOT)
# ──────────────────────────────────────────────────────────────────────────────
build_cli() {
    dotnet_publish "cli" \
        "$REPO_ROOT/src/Dotori.Cli/Dotori.Cli.csproj" \
        "$BUILD_DIR/cli"
}

# ──────────────────────────────────────────────────────────────────────────────
# 2. Language Server
# ──────────────────────────────────────────────────────────────────────────────
build_language_server() {
    dotnet_publish "language_server" \
        "$REPO_ROOT/src/Dotori.LanguageServer/Dotori.LanguageServer.csproj" \
        "$BUILD_DIR/language_server"
}

# ──────────────────────────────────────────────────────────────────────────────
# 3. Build Server
# ──────────────────────────────────────────────────────────────────────────────
build_build_server() {
    dotnet_publish "build_server" \
        "$REPO_ROOT/src/Dotori.BuildServer/Dotori.BuildServer.csproj" \
        "$BUILD_DIR/build_server"
}

# ──────────────────────────────────────────────────────────────────────────────
# 4. Worker
# ──────────────────────────────────────────────────────────────────────────────
build_worker() {
    dotnet_publish "worker" \
        "$REPO_ROOT/src/Dotori.Worker/Dotori.Worker.csproj" \
        "$BUILD_DIR/worker"
}

# ──────────────────────────────────────────────────────────────────────────────
# 5. Registry
# ──────────────────────────────────────────────────────────────────────────────
build_registry() {
    dotnet_publish "registry" \
        "$REPO_ROOT/src/Dotori.Registry/Dotori.Registry.csproj" \
        "$BUILD_DIR/registry"
}

# ──────────────────────────────────────────────────────────────────────────────
# 메인
# ──────────────────────────────────────────────────────────────────────────────
echo ""
echo -e "${CYAN}━━━ Dotori 빌드 ━━━${NC}  config=${CONFIG}"
echo ""

FAILED=()

run_target() {
    local target="$1"
    if should_build "$target"; then
        "build_$target" || { error "[$target] 빌드 실패"; FAILED+=("$target"); }
    else
        info "[$target] 건너뜀"
    fi
}

for t in "${ALL_TARGETS[@]}"; do
    run_target "$t"
done

echo ""
if [[ ${#FAILED[@]} -eq 0 ]]; then
    success "모든 빌드 완료 → $BUILD_DIR/"
else
    error "실패한 대상: ${FAILED[*]}"
    exit 1
fi
