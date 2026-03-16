# Dotori 프로젝트 빌드 방법

dotori 자체(CLI, 서버, 워커 등)를 소스에서 빌드하는 방법을 설명합니다.

## 사전 요구사항

- .NET 10 SDK 이상
- bash (Linux / macOS / WSL)

## 빌드 대상

| 대상 이름 | 설명 | 출력 경로 |
|-----------|------|-----------|
| `cli` | dotori CLI (NativeAOT 단일 실행 파일) | `build/cli/` |
| `language_server` | LSP 서버 | `build/language_server/` |
| `build_server` | 분산 빌드 코디네이터 | `build/build_server/` |
| `worker` | 분산 빌드 워커 | `build/worker/` |
| `registry` | 패키지 레지스트리 서버 | `build/registry/` |

---

## build.sh 사용법

```bash
./build.sh [옵션]
```

### 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `--only <targets>` | 전체 | 빌드할 대상을 쉼표로 지정 |
| `--skip <targets>` | 없음 | 건너뛸 대상을 쉼표로 지정 |
| `--config <cfg>` | `Release` | 빌드 구성 (`Release` / `Debug`) |
| `--rid <rid>` | — | .NET Runtime Identifier. 지정 시 self-contained로 퍼블리시 |
| `--version <ver>` | — | 어셈블리 버전 (예: `v1.2.3`) |
| `--help` | — | 도움말 출력 |

### 예시

```bash
# 전체 빌드 (Release)
./build.sh

# CLI와 Language Server만 빌드
./build.sh --only cli,language_server

# CLI를 linux-x64 self-contained 단일 바이너리로 빌드
./build.sh --only cli --rid linux-x64

# CLI + 서버 + 워커를 Windows x64용으로 빌드, 버전 태그 포함
./build.sh --only cli,build_server,worker --rid win-x64 --version v1.0.0

# macOS Apple Silicon용 CLI 빌드
./build.sh --only cli --rid osx-arm64

# Debug 구성으로 전체 빌드
./build.sh --config Debug

# registry를 제외하고 빌드
./build.sh --skip registry
```

---

## dotnet CLI로 직접 빌드

`build.sh`를 사용하지 않고 `dotnet` 명령으로 개별 컴포넌트를 빌드할 수 있습니다.

```bash
# 빌드 (출력: src/<프로젝트>/bin/)
dotnet build src/Dotori.Cli/Dotori.Cli.csproj

# 퍼블리시 — 현재 플랫폼
dotnet publish src/Dotori.Cli/Dotori.Cli.csproj -c Release -o build/cli

# 퍼블리시 — 크로스 컴파일 (linux-x64)
dotnet publish src/Dotori.Cli/Dotori.Cli.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o build/cli-linux-x64

# 실행 (개발 중)
dotnet run --project src/Dotori.Cli/Dotori.Cli.csproj -- build --help

# 테스트
dotnet test

# 코드 포매팅
dotnet csharpier format .
# (최초 실행 전: dotnet tool install csharpier)
```

---

## 지원 Runtime Identifier (RID)

| RID | 대상 |
|-----|------|
| `linux-x64` | Linux amd64 |
| `linux-arm64` | Linux arm64 |
| `win-x64` | Windows amd64 |
| `win-arm64` | Windows arm64 |
| `osx-arm64` | macOS Apple Silicon |
| `osx-x64` | macOS Intel |

RID를 지정하지 않으면 빌드 호스트 플랫폼에 맞는 바이너리가 생성됩니다.

---

## 솔루션 구조

```
Dotori.slnx
├── src/
│   ├── Dotori.Cli/              ← CLI 진입점 (NativeAOT)
│   ├── Dotori.Core/             ← 핵심 빌드 시스템 로직
│   ├── Dotori.Grpc/             ← gRPC 스텁 (proto/ 자동 생성)
│   ├── Dotori.PackageManager/   ← 패키지 매니저
│   ├── Dotori.BuildServer/      ← 분산 빌드 코디네이터
│   ├── Dotori.Worker/           ← 분산 빌드 워커
│   ├── Dotori.LanguageServer/   ← LSP 서버
│   └── Dotori.Registry/         ← 패키지 레지스트리
└── tests/
    ├── Dotori.Tests.Build/
    ├── Dotori.Tests.Graph/
    ├── Dotori.Tests.Parsing/
    ├── Dotori.Tests.PackageManager/
    ├── Dotori.Tests.LanguageServer/
    └── Dotori.Tests.Registry/
```
