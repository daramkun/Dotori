# CLI 사용법

dotori CLI의 모든 명령어와 옵션을 설명합니다.

## 전역 옵션

```
dotori --version    # 버전 출력
dotori --help       # 도움말 출력
```

---

## build — 빌드

프로젝트를 빌드합니다.

```bash
dotori build [옵션]
```

### 프로젝트 선택 옵션

| 옵션 | 설명 |
|------|------|
| `--project <경로>` | `.dotori` 파일 또는 해당 파일이 있는 디렉토리 경로를 직접 지정 |
| `--all` | 하위 디렉토리에서 발견된 모든 프로젝트를 프롬프트 없이 빌드 |

`--project`를 생략하면 다음 순서로 프로젝트를 탐색합니다.

1. 현재 디렉토리의 `.dotori`
2. 상위 디렉토리를 git root까지 재귀 탐색
3. 하위 디렉토리 탐색 후 목록 수집
4. 하나면 자동 선택, 둘 이상이면 대화형 선택 프롬프트

### 빌드 구성 옵션

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| `--release` | Debug | Release 구성으로 빌드 |
| `--target <타겟>` | 호스트 자동 감지 | 빌드 타겟 지정 (예: `macos-arm64`, `linux-x64`) |
| `--compiler <msvc\|clang>` | 자동 감지 | 컴파일러 선택 |
| `--runtime-link <static\|dynamic>` | static | 런타임 링크 방식 |
| `--libc <glibc\|musl>` | glibc | C 런타임 라이브러리 (Linux 전용) |
| `--stdlib <libc++\|libstdc++>` | 플랫폼별 기본값 | C++ 표준 라이브러리 |
| `--jobs <N>` | CPU 코어 수 | 병렬 빌드 작업 수 |
| `--no-modules` | — | C++ Modules 지원 비활성화 |

### 단일 파일 빌드 옵션

| 옵션 | 설명 |
|------|------|
| `--file <경로>` | 지정 파일만 컴파일 후 전체 링크 |
| `--file <경로> --no-link` | 컴파일만 수행 (링크 없음) |
| `--file <경로> --no-unity` | Unity Build가 켜진 경우에도 해당 파일을 단독으로 직접 컴파일 |

**Unity Build 환경에서의 `--file` 동작:**

- `--file src/foo.cpp` → `foo.cpp`가 포함된 unity batch 파일을 빌드 후 전체 링크
- `--file src/foo.cpp --no-link` → unity batch 파일만 컴파일 (`.obj` 생성)
- `--file src/foo.cpp --no-unity` → unity build를 무시하고 `foo.cpp` 단독 컴파일

**C++ Modules 파일 (`.cppm`, `.ixx`) 지정 시:**

- BMI만 재생성 (`--no-link` 암묵적 적용)
- 의존 BMI가 없으면 선행 BMI도 자동으로 함께 빌드

### 프로젝트 옵션 플래그

`.dotori` 파일에서 `option` 블록으로 선언된 옵션을 켜거나 끌 수 있습니다.

| 패턴 | 설명 |
|------|------|
| `--<옵션명>` | 해당 옵션을 활성화 (`default = false`인 옵션을 켤 때) |
| `--no-<옵션명>` | 해당 옵션을 비활성화 (`default = true`인 옵션을 끌 때) |

선언되지 않은 옵션 이름을 지정하면 오류가 출력됩니다.

```bash
# simd 옵션 활성화 (default=false 인 경우)
dotori build --simd

# simd 옵션 비활성화 (default=true 인 경우)
dotori build --no-simd

# 여러 옵션 동시 지정
dotori build --simd --experimental
```

### 분산 빌드 옵션

| 옵션 | 설명 |
|------|------|
| `--remote <URL>` | 분산 빌드 서버 주소 (예: `http://build-server:5100`) |

### 예시

```bash
# 현재 디렉토리 Debug 빌드
dotori build

# Release 빌드
dotori build --release

# iOS 크로스 컴파일 (macOS에서)
dotori build --target ios-arm64

# Android 크로스 컴파일
dotori build --target android-arm64

# Linux musl static 빌드 (완전 정적 바이너리)
dotori build --target linux-x64 --libc musl --runtime-link static

# WASM Emscripten 빌드
dotori build --target wasm32-emscripten

# 특정 소스 파일만 빌드
dotori build --file src/renderer.cpp

# 프로젝트 옵션 사용
dotori build --simd
dotori build --no-experimental

# 분산 빌드 서버 사용
dotori build --remote http://build-server:5100 --jobs 32
```

---

## run — 실행

프로젝트를 빌드하고 실행합니다. `type = executable` 프로젝트에서만 동작합니다.

```bash
dotori run [옵션] [-- <실행 인수>]
```

| 옵션 | 설명 |
|------|------|
| `--project <경로>` | 프로젝트 경로 지정 |
| `--release` | Release 빌드로 실행 |
| `--` | 이후 인수는 실행 파일에 전달 |

```bash
dotori run
dotori run --release
dotori run -- --config prod.json --verbose
```

---

## test — 테스트

테스트 대상 프로젝트를 빌드하고 실행합니다.

```bash
dotori test [옵션]
```

| 옵션 | 설명 |
|------|------|
| `--filter <패턴>` | 테스트 이름 필터 (글로브 패턴) |
| `--release` | Release 구성으로 테스트 |

```bash
dotori test
dotori test --filter "MyTest*"
```

---

## clean — 빌드 캐시 정리

```bash
dotori clean [옵션]
```

| 옵션 | 설명 |
|------|------|
| `--all` | 모든 프로젝트의 빌드 캐시 정리 |
| `--deps` | `deps/` 디렉토리(받아온 패키지)도 함께 삭제 |

`.dotori-cache/` 디렉토리와 `output { }` 블록에서 지정한 경로의 복사본도 함께 삭제됩니다.
`--deps` 플래그를 추가하면 `deps/` 디렉토리도 삭제되며, 다음 빌드 시 패키지를 다시 받아옵니다.

```bash
dotori clean              # 빌드 캐시만 삭제
dotori clean --deps       # 빌드 캐시 + 받아온 패키지(deps/) 삭제
dotori clean --all --deps # 모든 프로젝트의 빌드 캐시 + deps/ 삭제
```

---

## 패키지 관리 명령어

### add — 의존성 추가

```bash
dotori add <패키지명>              # 최신 버전 추가
dotori add <패키지명>@1.2.0        # 특정 버전 추가
dotori add <URL> --git             # git URL에서 추가
dotori add <경로> --path           # 로컬 경로에서 추가
```

### remove — 의존성 제거

```bash
dotori remove <패키지명>
```

### update — 의존성 업데이트

```bash
dotori update              # 모든 의존성 업데이트
dotori update <패키지명>   # 특정 패키지만 업데이트
```

### list — 설치된 패키지 목록

```bash
dotori list
```

---

## format — 포매팅

`.dotori` 파일을 정규 형식으로 포매팅합니다.

```bash
dotori format [파일] [옵션]
```

| 옵션 | 설명 |
|------|------|
| `<파일>` | 포매팅할 `.dotori` 파일 또는 디렉토리 경로 (생략 시 현재 디렉토리 탐색) |
| `--project <경로>` | `.dotori` 파일 또는 프로젝트 디렉토리 경로 |
| `--check` | 파일을 수정하지 않고 포매팅 여부만 확인 (포매팅 필요 시 exit 1) |
| `--stdout` | 파일을 수정하지 않고 포매팅 결과를 stdout으로 출력 |

> **참고:** 포매터는 AST 기반으로 동작하므로 원본 주석(`(*...*)`)은 보존되지 않습니다.

```bash
# 현재 디렉토리의 .dotori 파일 포매팅
dotori format

# 특정 파일 포매팅
dotori format .dotori

# 포매팅 여부 확인 (CI 사용 가능)
dotori format --check

# stdout으로 출력
dotori format --stdout .dotori
```

---

## 정보 및 진단 명령어

### info — 프로젝트 정보

현재 프로젝트의 이름, 타입, 소스, 의존성 등을 출력합니다.

```bash
dotori info
dotori info --project ./lib
```

### graph — 의존성 그래프 출력

프로젝트 DAG와 패키지 의존성 그래프를 텍스트 형식으로 출력합니다.

```bash
dotori graph
```

### check — 유효성 검사

`.dotori` 파일의 문법 오류, 이식성 경고 등을 검사합니다.

```bash
dotori check
```

**검사 항목:**

- DSL 문법 오류
- `path` 의존성 대상 파일 존재 여부
- 조건 블록 없이 `compile-flags`/`link-flags` 사용 시 이식성 경고
  - 예: `warning: compile-flags used without a compiler condition (e.g. [msvc], [clang]) — may reduce portability`

### targets — 지원 타겟 목록

현재 호스트에서 빌드 가능한 타겟 목록을 출력합니다.

```bash
dotori targets
```

### toolchain — 감지된 툴체인 정보

설치된 컴파일러, SDK, 링커 등의 정보를 출력합니다.

```bash
dotori toolchain
```

---

## Language Server 명령어

IDE 플러그인이 내부적으로 호출합니다.

```bash
dotori lsp                              # stdio transport (LSP 3.17)
dotori lsp --log-file /tmp/dotori.log   # 디버그 로그 출력
```

---

## generate-compile-commands — compile_commands.json 생성

clangd 등 C++ IntelliSense 도구와 통합하기 위한 `compile_commands.json`을 생성합니다.

```bash
dotori generate-compile-commands                      # 호스트 기본 타겟
dotori generate-compile-commands --target linux-x64   # 타겟 명시
dotori generate-compile-commands --output ./          # 출력 경로 지정
```

멀티 프로젝트 환경에서는 모든 프로젝트 항목이 하나의 파일에 병합됩니다.

---

## 레지스트리 명령어

### login / logout

```bash
dotori login                          # 기본 레지스트리에 로그인
dotori login --registry https://my.registry.example.com
dotori logout
```

### publish — 패키지 배포

`.dotori` 파일에 `package { }` 블록이 있어야 합니다.

```bash
dotori publish
dotori publish --registry https://my.registry.example.com

# 레지스트리 대신 로컬 디렉토리에 배포
dotori publish --prefix ./local-repo
dotori publish --prefix /opt/packages
```

| 옵션 | 설명 |
|------|------|
| `--project <경로>` | `.dotori` 파일 또는 프로젝트 디렉토리 경로 |
| `--registry <URL>` | 배포할 레지스트리 URL (기본값: 설정 파일의 기본 레지스트리) |
| `--prefix <경로>` | 레지스트리 대신 로컬 디렉토리에 배포. `<prefix>/<name>/<version>/` 구조로 저장 |
| `--dry-run` | 아카이브만 생성하고 업로드·복사 없이 종료 |

`--registry`와 `--prefix`는 동시에 사용할 수 없습니다.

### search — 패키지 검색

```bash
dotori search <키워드>
```

### owner — 패키지 소유자 관리

```bash
dotori owner add <패키지명> <사용자명>
dotori owner remove <패키지명> <사용자명>
dotori owner list <패키지명>
```

### yank / unyank — 버전 비활성화

```bash
dotori yank <패키지명>@<버전>       # 버전 비활성화 (취약 버전 처리)
dotori unyank <패키지명>@<버전>     # 비활성화 취소
```

---

## 환경 변수

빌드 시 다음 환경 변수가 자동으로 설정됩니다. `pre-build`, `post-build` 스크립트와 DSL 문자열에서 참조할 수 있습니다.

| 변수명 | 예시 값 | 설명 |
|--------|---------|------|
| `DOTORI_TARGET` | `macos-arm64` | 전체 빌드 타겟 ID |
| `DOTORI_CONFIG` | `debug`, `release` | 빌드 구성 |
| `DOTORI_PLATFORM` | `windows`, `linux`, `macos` | 타겟 플랫폼 |
| `DOTORI_ARCH` | `x64`, `arm64`, `wasm32` | 타겟 CPU 아키텍처 |
| `DOTORI_PROJECT_DIR` | `/path/to/project` | 프로젝트 루트 절대 경로 |
| `DOTORI_OUTPUT_DIR` | `.dotori-cache/obj/...` | 링크 결과물 위치 |

---

## 캐시 디렉토리 구조

```
.dotori-cache/               ← 프로젝트 로컬 캐시
├── hashes.db                ← 증분 빌드용 해시 DB
├── bmi/                     ← C++ Modules BMI 파일
│   ├── MyLib.ifc            #   MSVC
│   └── MyLib.pcm            #   Clang
├── unity/                   ← Unity Build 임시 파일
│   └── unity_0.cpp
└── obj/                     ← 타겟별 오브젝트 파일
    ├── macos-arm64-debug/
    ├── macos-arm64-release/
    ├── linux-x64-glibc-static-debug/
    └── wasm32-emscripten-release/

deps/                        ← 받아온 패키지 (dotori clean --deps 로 삭제)
├── fmt/
│   ├── .dotori
│   └── .dotori-cache/     # 이 프로젝트에서만 사용하는 빌드 캐시
└── spdlog/
    ├── .dotori
    └── .dotori-cache/
```

> **참고**: 패키지는 전역 캐시(`~/.dotori/packages/`) 없이 각 프로젝트의 `deps/` 폴더에 독립적으로 저장됩니다.
> 이를 통해 프로젝트마다 다른 빌드 옵션(defines, compile-flags 등)으로 동일 패키지를 빌드해도 캐시가 충돌하지 않습니다.
