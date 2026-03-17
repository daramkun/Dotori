# dotori — C++ 빌드 시스템 + 패키지 매니저 작업내역서

> **작성일**: 2026-03-08
> **버전**: v1
> **구현 언어**: C# (.NET 8+, NativeAOT 배포)
> **목표**: Cargo 수준의 UX를 C++에 제공하는 독립형 빌드 시스템 + 패키지 매니저
> **외부 호환성**: 의도적으로 제외 (vcpkg/Conan/CMake 브리지 없음)
> **컴파일러 지원**: MSVC, Clang (clang-cl 포함), MinGW (크로스 컴파일)

---

## 1. 도구 개요

| 항목 | 내용 |
|------|------|
| 도구 이름 | **dotori** |
| 구현 언어 | C# (.NET 10, NativeAOT 단일 실행 파일) |
| 프로젝트 파일 | `.dotori` (디렉토리당 하나) |
| 패키지 명세 | `.dotori` (type = package, 동일 파일) |
| Lock 파일 | `.dotori.lock` (자동 생성) |
| 빌드 백엔드 | 직접 컴파일러 호출 (MSVC / Clang / emcc) |
| C++ 기본 표준 | C++23 |
| C++ Modules | 지원 (MSVC + Clang) |
| 기본 Configuration | Debug / Release |
| 기본 런타임 링크 | Static |
| 의존성 소스 | git (태그/커밋) + 로컬 path |
| 빌드 순서 | dependencies 블록 기반 자동 DAG 구성 |
| 분산 빌드 | 원격 빌드 서버 방식 (Phase 2~3) |

---

## 2. 프로젝트 파일 설계

### 2-1. 파일 구조 원칙

- 모든 파일은 확장자 `.dotori` 고정
- 디렉토리당 `.dotori` 파일은 **하나만** 허용
- 파일 내 최상위 키워드가 `project` 이면 빌드 대상, `package` 이면 패키지 명세
- 한 파일에 `project` + `package` 블록을 함께 작성 가능 (라이브러리 배포 시)

```
my-app/
└── .dotori          ← project MyApp { ... }

my-lib/
└── .dotori          ← project MyLib { ... }
                        package { ... }   ← 배포 시 함께 작성

subdir/
└── .dotori          ← project SubLib { ... }
```

### 2-2. 멀티 프로젝트 레이아웃

솔루션 파일 없이 `path` 의존성으로 프로젝트 간 관계를 표현합니다.
빌드 순서는 의존성 그래프(DAG)에서 자동으로 결정됩니다.

```
repo/
├── app/
│   └── .dotori      ← dependencies { my-lib = { path = "../lib" } }
└── lib/
    └── .dotori      ← project MyLib { ... }
```

`dotori build` 를 `app/` 에서 실행하면:

1. `app/.dotori` 로드
2. `path = "../lib"` 의존성 발견 → `lib/.dotori` 로드
3. DAG: `MyLib` → `MyApp` 순서로 빌드

### 2-3. 프로젝트 파일 예시

```
# app/.dotori
project MyApp {
    type        = executable
    std         = c++23
    description = "My application"

    sources {
        include "src/**/*.cpp"
        include "src/**/*.cppm"
    }

    modules {
        include "src/**/*.cppm"
        include "src/**/*.ixx"
    }

    headers {
        public  "include/"
        private "src/"
    }

    runtime-link = static

    [windows] {
        sources { include "src/platform/windows/**/*.cpp" }
        defines { "PLATFORM_WINDOWS" "WIN32_LEAN_AND_MEAN" }
        links   { "kernel32" "user32" "ole32" }
    }

    [uwp] {
        sources { include "src/platform/uwp/**/*.cpp" }
        defines { "PLATFORM_UWP" "WINAPI_FAMILY=WINAPI_FAMILY_APP" }
        # runtime-link = dynamic 강제 적용
    }

    [linux] {
        sources { include "src/platform/linux/**/*.cpp" }
        defines { "PLATFORM_LINUX" }
        links   { "pthread" "dl" }
        libc    = glibc
        stdlib  = libstdc++
    }

    [android] {
        sources           { include "src/platform/android/**/*.cpp" }
        defines           { "PLATFORM_ANDROID" }
        android-api-level = 26
    }

    [macos] {
        sources    { include "src/platform/macos/**/*.cpp" }
        defines    { "PLATFORM_MACOS" }
        frameworks { "Foundation" "Metal" "MetalKit" }
        macos-min  = "12.0"
    }

    [ios] {
        sources    { include "src/platform/apple/**/*.cpp" }
        defines    { "PLATFORM_IOS" }
        frameworks { "Foundation" "UIKit" "Metal" }
        ios-min    = "15.0"
        # runtime-link = static 강제 적용
    }

    [wasm] {
        sources { include "src/platform/wasm/**/*.cpp" }
        defines { "PLATFORM_WASM" }
        # runtime-link 설정 무시, 항상 static
    }

    [wasm.emscripten] {
        emscripten-flags { "-sUSE_SDL=2" "-sALLOW_MEMORY_GROWTH" }
    }

    [debug] {
        defines    { "DEBUG" "_DEBUG" }
        optimize   = none
        debug-info = full
    }

    [release] {
        defines    { "NDEBUG" }
        optimize   = speed
        debug-info = none
        lto        = true
    }

    dependencies {
        my-lib = { path = "../lib" }     # 로컬 프로젝트 — 빌드 순서 자동 결정
        fmt    = "10.2.0"                # 패키지 매니저
        spdlog = { git = "https://github.com/gabime/spdlog", tag = "v1.13.0" }
    }

    pch {
        header = "src/pch.h"
        source = "src/pch.cpp"
    }

    unity-build {
        enabled    = true
        batch-size = 8
        exclude    { "src/main.cpp" }
    }

    output {
        binaries  = "bin/"    # exe, dll/so/dylib 복사 위치 — 프로젝트 루트 기준 상대경로
        libraries = "lib/"    # .lib(import), .a(static) 복사 위치
        symbols   = "pdb/"    # .pdb, .dSYM 복사 위치
    }

    # 사용자 정의 컴파일러/링커 플래그 — 컴파일러별로 다른 값 지정
    [msvc] {
        compile-flags { "/arch:AVX2" "/fp:fast" }
        link-flags    { "/SUBSYSTEM:WINDOWS" "/OPT:REF" "/OPT:ICF" }
    }

    [clang] {
        compile-flags { "-march=native" "-ffast-math" }
    }

    [clang.linux] {
        link-flags { "-Wl,--as-needed" "-Wl,--gc-sections" }
    }

    [clang.macos] {
        link-flags { "-Wl,-rpath,@executable_path/lib" }
    }

    [release.msvc] {
        compile-flags { "/Oi" "/Ot" }
        link-flags    { "/LTCG" }
    }

    pre-build {
        "scripts/gen_version.sh"
        "scripts/download_assets.sh --quiet"
    }

    post-build {
        "scripts/sign.sh --cert certs/release.p12"
    }
}
```

```
# lib/.dotori — project + package 함께 작성
project MyLib {
    type = static-library
    std  = c++23

    sources { include "src/**/*.cpp" }
    modules { include "src/**/*.cppm" }

    headers {
        public  "include/"
        private "src/"
    }

    [windows.shared-library] {
        defines { "MYLIB_EXPORTS" }
    }
}

package {
    name        = "my-lib"
    version     = "1.0.0"
    description = "My C++ library"
    license     = "MIT"
}
```

### 2-4. Lock 파일 (`.dotori.lock`, 자동 생성)

```
lock-version = 1

[[package]]
name    = "fmt"
version = "10.2.0"
source  = "git+https://github.com/fmtlib/fmt#v10.2.0"
hash    = "sha256:abcdef1234..."

[[package]]
name    = "spdlog"
version = "1.13.0"
source  = "git+https://github.com/gabime/spdlog#v1.13.0"
hash    = "sha256:fedcba4321..."
deps    = ["fmt@10.2.0"]
```

---

## 3. 프로젝트 탐색 및 선택 UX

### 3-1. 탐색 순서

`dotori build` (또는 `run`, `test` 등) 실행 시:

```
1. --project <path> 옵션이 있으면 해당 경로의 .dotori 사용
2. 현재 디렉토리에 .dotori 가 있으면 사용
3. 없으면 상위 디렉토리를 재귀적으로 탐색 (git root까지)
4. 그래도 없으면 하위 디렉토리를 탐색하여 .dotori 목록 수집
5. 목록이 하나면 자동 선택
6. 둘 이상이면 대화형 선택 프롬프트 표시
7. 하나도 없으면 오류 출력
```

### 3-2. 대화형 선택 프롬프트

번호 입력 → 해당 프로젝트만 빌드 / Enter → 전체 빌드 / `--all` 플래그 → 프롬프트 없이 전체 빌드

### 3-3. 직접 경로 지정

```bash
dotori build --project ./lib/.dotori
dotori build --project ../other-repo
dotori run   --project ./app
```

`--project` 는 `.dotori` 파일 경로 또는 `.dotori` 가 있는 디렉토리 경로 모두 허용합니다.

---

## 4. 빌드 순서 자동화 (의존성 기반 DAG)

### 4-1. 규칙

- `dependencies` 블록의 `path =` 항목만 빌드 순서에 영향을 줍니다
- `git =` / `version =` 항목은 패키지 매니저가 별도 처리하며 빌드 순서와 무관합니다
- DAG 구성은 빌드 시작 시 모든 `path` 의존성을 재귀적으로 탐색하여 수행합니다
- 순환 의존성 감지 시 명확한 오류를 출력합니다

### 4-2. 전체 빌드 시 독립 프로젝트 병렬화

DAG에서 의존 관계가 없는 프로젝트들은 동시에 빌드합니다.
예: `lib`와 `tools`가 모두 `core`에만 의존한다면, `core` 완료 후 `lib`와 `tools`를 병렬로 빌드합니다.

---

## 5. 빌드 타겟 & 런타임 매트릭스

### 5-1. 지원 타겟 전체 목록

| 타겟 ID | OS | 아키텍처 | 컴파일러 | 비고 |
|---------|----|---------|---------|------|
| `windows-x64` | Windows | amd64 | MSVC / Clang | 데스크탑 |
| `windows-x86` | Windows | x86 | MSVC / Clang | 32bit |
| `windows-arm64` | Windows | arm64 | MSVC / Clang | |
| `uwp-x64` | UWP | amd64 | MSVC | `/ZW` + WinRT |
| `uwp-arm64` | UWP | arm64 | MSVC | `/ZW` + WinRT |
| `linux-x64` | Linux | amd64 | Clang | |
| `linux-arm64` | Linux | arm64 | Clang | |
| `android-arm64` | Android | arm64 | NDK Clang | API Level 지정 |
| `android-x64` | Android | amd64 | NDK Clang | |
| `android-arm` | Android | armv7 | NDK Clang | |
| `macos-arm64` | macOS | arm64 | Clang | |
| `macos-x64` | macOS | amd64 | Clang | Intel Mac |
| `ios-arm64` | iOS | arm64 | Clang 크로스 | Xcode 불필요 |
| `ios-sim-arm64` | iOS Simulator | arm64 | Clang 크로스 | |
| `tvos-arm64` | tvOS | arm64 | Clang 크로스 | |
| `watchos-arm64_32` | watchOS | arm64_32 | Clang 크로스 | |
| `wasm32-emscripten` | WASM | wasm32 | emcc | Emscripten |
| `wasm32-bare` | WASM | wasm32 | Clang | wasm32-unknown-unknown |

### 5-2. 런타임 매트릭스

#### C 런타임 (libc)

| 타겟 | glibc | musl | 비고 |
|------|-------|------|------|
| `linux-*` | ✅ 기본 | ✅ | musl + static = 완전 정적 바이너리 |
| `android-*` | ✅ Bionic | ❌ | Bionic = glibc 계열 |
| `windows-*` | ❌ | ❌ | MSVCRT / UCRT |
| `macos-*` / `ios-*` 등 | ❌ | ❌ | libSystem |
| `wasm32-*` | ❌ | ❌ | wasi-libc / Emscripten libc |

#### C++ 표준 라이브러리

| 타겟 | libc++ | libstdc++ | MSVC STL |
|------|--------|-----------|----------|
| `linux-*` | ✅ | ✅ 기본 | ❌ |
| `android-*` | ✅ 고정 | ❌ | ❌ |
| `windows-*` (MSVC / clang-cl) | ❌ | ❌ | ✅ 기본 |
| `windows-*` (Clang, no-MSVC) | ✅ | ❌ | ❌ |
| `macos-*` / Apple | ✅ 기본 | ❌ | ❌ |
| `wasm32-*` | ✅ 기본 | ❌ | ❌ |

#### 런타임 링크 (Static / Dynamic)

| 타겟 | Static | Dynamic | 기본값 | 비고 |
|------|--------|---------|--------|------|
| `windows-*` | ✅ `/MT` | ✅ `/MD` | Static | |
| `uwp-*` | ❌ | ✅ 강제 | Dynamic | UWP 정책 |
| `linux-*` | ✅ | ✅ | Static | |
| `android-*` | ✅ | ✅ | Static | |
| `macos-*` | ✅ 부분 | ✅ | Static | libSystem 제외 |
| `ios-*` / `tvos-*` / `watchos-*` | ✅ 강제 | ❌ | Static | App Store 정책 |
| `wasm32-*` | ✅ 고정 | ❌ | Static | 설정 무시 |

---

## 6. DSL 문법 명세 (EBNF)

### 6-0. 환경변수 보간 (`${VAR}`)

DSL의 모든 사용자 작성 문자열 값 안에서 `${VAR}` 형태로 환경변수를 참조할 수 있습니다.
보간은 빌드 시작 시(`ProjectFlattener.Flatten()`) 수행됩니다.

```
headers { public "${BOOST_ROOT}/include/" }
sources { include "${SRC_DIR}/**/*.cpp" }
compile-flags { "-DAPP_VERSION=${APP_VERSION}" }
link-flags    { "-L${MY_SDK}/lib" }
defines   { "SDK_PATH=${MY_SDK}" }
pre-build { "${SCRIPTS_DIR}/gen_version.sh" }
dependencies {
    mylib = { path = "${MYLIB_ROOT}" }
}
```

| 항목 | 동작 |
|------|------|
| `${VAR}` | 환경변수 `VAR` 값으로 치환 |
| 정의되지 않은 변수 | 빈 문자열로 치환 |
| 닫히지 않은 `${` | 리터럴로 유지 |

**미적용 대상** (키워드/enum 값): `type`, `std`, `optimize`, `debug-info`, `runtime-link` 등 DSL 키워드는 환경변수 확장 없이 그대로 파싱됩니다.

**dotori 자동 주입 변수** (`MakeTargetContext` 호출 시 프로세스 환경에 자동 설정):

| 변수명 | 예시 값 | 설명 |
|--------|---------|------|
| `DOTORI_TARGET`   | `macos-arm64`, `windows-x64` | 전체 빌드 타겟 ID |
| `DOTORI_CONFIG`   | `debug`, `release`           | 빌드 구성 |
| `DOTORI_PLATFORM` | `windows`, `linux`, `macos`, `ios`, `android`, `wasm32` | 타겟 OS/플랫폼 |
| `DOTORI_ARCH`     | `x64`, `x86`, `arm64`, `arm`, `arm64_32`, `wasm32`      | 타겟 CPU 아키텍처 |

사용 예:
```
defines { "TARGET_PLATFORM=${DOTORI_PLATFORM}" "ARCH=${DOTORI_ARCH}" }
output  { binaries = "bin/${DOTORI_TARGET}-${DOTORI_CONFIG}/" }
pre-build { "scripts/codegen.sh --arch=${DOTORI_ARCH}" }
```

```ebnf
file            ::= { top_decl }
top_decl        ::= project_decl | package_decl

ident           ::= [a-zA-Z_][a-zA-Z0-9_\-]*
string          ::= '"' { char } '"'   # ${VAR} 보간 적용 대상
integer         ::= [0-9]+
bool_val        ::= "true" | "false"
version_str     ::= string
path_glob       ::= string

dep_value       ::= version_str
                  | "{" dep_option { "," dep_option } "}"
dep_option      ::= "git"     "=" string
                  | "tag"     "=" string
                  | "commit"  "=" string
                  | "path"    "=" string      # 빌드 순서 DAG에 포함
                  | "version" "=" version_str

condition       ::= "[" cond_expr "]"
cond_expr       ::= cond_atom { "." cond_atom }
cond_atom       ::= platform_cond | config_cond | compiler_cond | runtime_cond | ident

platform_cond   ::= "windows" | "uwp" | "linux" | "android"
                  | "macos" | "ios" | "tvos" | "watchos" | "wasm"
config_cond     ::= "debug" | "release" | ident
compiler_cond   ::= "msvc" | "clang"
runtime_cond    ::= "static" | "dynamic"
                  | "glibc"  | "musl"
                  | "libcxx" | "libstdcxx"
                  | "emscripten" | "bare"

project_decl    ::= "project" ident "{" { project_item } "}"
project_item    ::= project_prop
                  | sources_block
                  | modules_block
                  | headers_block
                  | defines_block
                  | links_block
                  | frameworks_block
                  | compile_flags_block
                  | link_flags_block
                  | dependencies_block
                  | pch_block
                  | unity_build_block
                  | output_block
                  | pre_build_block
                  | post_build_block
                  | condition "{" { project_item } "}"

project_prop    ::= "type"               "=" project_type
                  | "std"                "=" cxx_std
                  | "description"        "=" string
                  | "optimize"           "=" optimize_level
                  | "debug-info"         "=" debug_info_level
                  | "runtime-link"       "=" runtime_link
                  | "libc"               "=" libc_type
                  | "stdlib"             "=" stdlib_type
                  | "lto"                "=" bool_val
                  | "warnings"           "=" warning_level
                  | "warnings-as-errors" "=" bool_val
                  | "android-api-level"  "=" integer
                  | "macos-min"          "=" string
                  | "ios-min"            "=" string
                  | "tvos-min"           "=" string
                  | "watchos-min"        "=" string
                  | "emscripten-flags"   "{" { string } "}"

project_type    ::= "executable" | "static-library" | "shared-library" | "header-only"
cxx_std         ::= "c++17" | "c++20" | "c++23"
optimize_level  ::= "none" | "size" | "speed" | "full"
debug_info_level::= "none" | "minimal" | "full"
runtime_link    ::= "static" | "dynamic"
libc_type       ::= "glibc" | "musl"
stdlib_type     ::= "libc++" | "libstdc++"
warning_level   ::= "none" | "default" | "all" | "extra"

sources_block   ::= "sources"    "{" { source_item } "}"
modules_block   ::= "modules"    "{" { source_item } [ "export-map" "=" bool_val ] "}"
source_item     ::= ( "include" | "exclude" ) path_glob

headers_block   ::= "headers"    "{" { header_item } "}"
header_item     ::= ( "public" | "private" ) string

defines_block   ::= "defines"       "{" { string | ident } "}"
links_block     ::= "links"         "{" { string | ident } "}"
frameworks_block::= "frameworks"    "{" { string | ident } "}"
compile_flags_block ::= "compile-flags" "{" { string } "}"
link_flags_block    ::= "link-flags"    "{" { string } "}"

(* compile-flags: 각 소스 파일 컴파일 시 dotori 생성 플래그 뒤에 추가.
   link-flags:    링크 단계에서 dotori 생성 플래그 뒤에 추가.
   두 블록 모두 조건 블록과 함께 사용하는 것을 권장.
   조건 없이 사용하면 `dotori check` 에서 이식성 경고를 출력함.
   DSL 속성(std, optimize, runtime-link 등)으로 이미 제어 가능한 값을
   compile-flags/link-flags 로 중복 지정하면 충돌 가능성이 있음. *)

dependencies_block ::= "dependencies" "{" { dep_item } "}"
dep_item        ::= ident "=" dep_value

pch_block       ::= "pch" "{" { pch_prop } "}"
pch_prop        ::= ( "header" | "source" ) "=" string
                  | "modules" "=" bool_val

unity_build_block ::= "unity-build" "{" { unity_prop } "}"
unity_prop      ::= "enabled"    "=" bool_val
                  | "batch-size" "=" integer
                  | "exclude"    "{" { path_glob } "}"

output_block    ::= "output" "{" { output_prop } "}"
output_prop     ::= "binaries"  "=" string   # exe, dll/so/dylib 복사 경로
                  | "libraries" "=" string   # .lib(import), .a(static) 복사 경로
                  | "symbols"   "=" string   # .pdb, .dSYM 복사 경로

(* 경로는 .dotori 파일 기준 상대 경로. 빌드는 .dotori-cache/ 내에서 수행되고,
   완료 후 지정 경로로 복사됨. 지정하지 않으면 복사하지 않음.
   조건 블록 적용 가능: [release] { output { binaries = "dist/" } } *)

pre_build_block  ::= "pre-build"  "{" { string } "}"
post_build_block ::= "post-build" "{" { string } "}"

(* 각 문자열은 실행할 명령어. 순서대로 실행되며, 종료 코드 != 0 이면 빌드 실패.
   실행 디렉토리: 프로젝트 루트 (.dotori 위치).
   조건 블록 적용 가능: [windows] { pre-build { "gen.bat" } }
   전달되는 환경변수:
     DOTORI_TARGET      — 빌드 타겟 (예: macos-arm64)
     DOTORI_CONFIG      — Debug | Release
     DOTORI_PROJECT_DIR — 프로젝트 루트 절대 경로
     DOTORI_OUTPUT_DIR  — 링크 결과물 위치 (.dotori-cache/obj/<target>-<config>/) *)

package_decl    ::= "package" "{" { package_item } "}"
package_item    ::= "name"        "=" string
                  | "version"     "=" version_str
                  | "description" "=" string
                  | "license"     "=" string
                  | "homepage"    "=" string
                  | authors_block
                  | exports_block
authors_block   ::= "authors" "{" { string } "}"
exports_block   ::= "exports" "{" { ident "=" string } "}"
```

---

## 7. 타겟별 툴체인 설계

### 7-1. Windows (`windows-*`)

Windows 빌드는 세 가지 모드를 지원합니다.

#### 모드 A — MSVC (`cl.exe`)

```
탐색: vswhere.exe → VCINSTALLDIR 환경변수
컴파일러: cl.exe
링커:     link.exe (MSVC)
ABI:      MSVC (x86_64-pc-windows-msvc)
STL:      MSVC STL (기본, 변경 불가)
런타임 링크:
  static  → /MT (debug: /MTd)
  dynamic → /MD (debug: /MDd)
```

#### 모드 B — clang-cl + Windows SDK (Windows 호스트)

clang-cl이 PATH에 있고 MSVC SDK가 감지될 때 활성화됩니다.
MsvcDriver 플래그(`/std:c++latest`, `/O2`, `/MT` 등)를 그대로 사용하며
clang-cl이 이를 투명하게 수락합니다.

```
탐색: PATH clang-cl.exe + vswhere.exe로 MSVC SDK 경로 확인
컴파일러: clang-cl.exe
링커:     lld-link.exe
ABI:      MSVC (x86_64-pc-windows-msvc)
STL:      MSVC STL
런타임 링크: /MT / /MD (MSVC와 동일)

추가 컴파일 플래그 (cl.exe와 달리 명시 필요):
  -imsvc "<VcToolsDir>/include"
  -imsvc "<WinSdkDir>/Include/<ver>/ucrt"
  -imsvc "<WinSdkDir>/Include/<ver>/um"
  -imsvc "<WinSdkDir>/Include/<ver>/shared"
```

#### 모드 C — clang++ + MinGW (크로스 컴파일 또는 Windows 호스트에서 MSVC SDK 없는 경우)

Windows 타겟을 macOS·Linux 호스트에서 빌드하거나, Windows 호스트에 MSVC SDK 없이
Clang만 설치된 경우에 사용됩니다. 자세한 내용은 §7-8 참조.

```
탐색 (비-Windows 호스트):
  1. PATH에서 <triple>-clang++ 탐색 (llvm-mingw)
  2. PATH clang++ + MinGW sysroot 탐색
     - MINGW_SYSROOT 환경변수
     - Homebrew: /opt/homebrew/opt/mingw-w64/toolchain-<arch>/
                 /usr/local/opt/mingw-w64/toolchain-<arch>/
     - Linux:    /usr/<arch>-w64-mingw32/

컴파일러: clang++ (또는 <triple>-clang++)
링커:     lld (via -fuse-ld=lld)
ABI:      MinGW (x86_64-w64-mingw32)
STL:      libstdc++ (MinGW 내장)
런타임 링크 (static):  -static-libgcc -static-libstdc++
출력:     실행 파일 .exe, 정적 라이브러리 .a (MinGW 관례)
```

### 7-2. UWP (`uwp-*`)

```
컴파일러: MSVC 전용
추가 플래그: /ZW /EHsc
추가 링크: WindowsApp.lib
runtime-link: dynamic 강제 (static 설정 시 경고 후 자동 전환)
```

### 7-3. Linux (`linux-*`)

```
컴파일러: clang++
타겟 트리플:
  glibc: x86_64-unknown-linux-gnu / aarch64-unknown-linux-gnu
  musl:  x86_64-unknown-linux-musl / aarch64-unknown-linux-musl

stdlib:
  libstdc++ → -stdlib=libstdc++ (기본)
  libc++    → -stdlib=libc++

런타임 링크:
  static (libstdc++) → -static-libgcc -static-libstdc++
  static (libc++)    → -static-libstdc++ -lc++abi
  musl + static      → 위 + -static → 완전 정적 바이너리

링커: ld.lld
```

### 7-4. Android (`android-*`)

```
NDK 탐색: ANDROID_NDK_HOME → ANDROID_HOME/ndk → 오류
컴파일러: $NDK/toolchains/llvm/prebuilt/<host>/bin/clang++

타겟 트리플:
  android-arm64 → aarch64-linux-android<API>
  android-x64   → x86_64-linux-android<API>
  android-arm   → armv7a-linux-androideabi<API>

libc: Bionic (선택 불가)
stdlib: libc++ 고정 (NDK r18+)
sysroot: $NDK/toolchains/llvm/prebuilt/<host>/sysroot
링커: ld.lld (NDK 내장)
```

### 7-5. macOS (`macos-*`)

```
컴파일러: clang++ (xcrun --find clang++)
SDK: xcrun --sdk macosx --show-sdk-path
stdlib: libc++ 기본 (libstdc++ 미지원)
링커: ld (Apple ld)
```

### 7-6. iOS / tvOS / watchOS (Clang 크로스, Xcode 불필요)

```
컴파일러: clang++ (Apple Clang)
SDK: xcrun --sdk <iphoneos|appletvos|watchos> --show-sdk-path

타겟 트리플:
  ios-arm64       → arm64-apple-ios<min>
  ios-sim-arm64   → arm64-apple-ios<min>-simulator
  tvos-arm64      → arm64-apple-tvos<min>
  watchos-arm64_32→ arm64_32-apple-watchos<min>

stdlib: libc++ 고정
runtime-link: static 강제 (App Store 정책)
링커: ld (Apple ld — Xcode Command Line Tools 필요)
```

### 7-7. WASM (`wasm32-*`)

```
[emscripten]
  컴파일러: emcc (EMSDK_PATH)
  출력: .wasm + .js

[bare]
  컴파일러: clang++
  타겟: wasm32-unknown-unknown
  추가 플래그: --no-standard-libraries -Wl,--no-entry -Wl,--export-all
  출력: .wasm

런타임 링크: static 고정 (설정 무시)
```

### 7-8. 크로스 컴파일 — Windows 타겟 (macOS / Linux 호스트)

macOS 또는 Linux에서 `dotori build --target windows-x64` 등을 실행하면
MinGW 기반 크로스 컴파일 툴체인을 자동 탐색합니다.

#### 툴체인 탐색 순서

```
1. PATH에서 <triple>-clang++ 탐색 (llvm-mingw 설치 시)
   예: x86_64-w64-mingw32-clang++, aarch64-w64-mingw32-clang++

2. PATH clang++ + MinGW sysroot 탐색
   sysroot 탐색 순서:
     a. MINGW_SYSROOT 환경변수
     b. Homebrew (Apple Silicon): /opt/homebrew/opt/mingw-w64/toolchain-<arch>/
     c. Homebrew (Intel Mac):     /usr/local/opt/mingw-w64/toolchain-<arch>/
     d. Linux 시스템 MinGW:       /usr/<arch>-w64-mingw32/

지원 아키텍처:
  windows-x64   → x86_64-w64-mingw32
  windows-arm64 → aarch64-w64-mingw32
  windows-x86   → 미지원 (MinGW32 크로스 컴파일 미지원)
```

#### 설치 예시

```bash
# macOS (Homebrew)
brew install llvm-mingw      # 권장: clang 기반, C++ Modules 미지원
brew install mingw-w64       # 대안: GCC 기반 (GCC 미지원, clang++ + sysroot로 사용)

# Linux (Debian/Ubuntu)
apt install gcc-mingw-w64 llvm clang   # clang + MinGW sysroot
```

#### 제약 사항

| 항목 | 상태 | 비고 |
|------|------|------|
| C++ Modules | ❌ 미지원 | MinGW 환경에서 P1689 스캔 불가 |
| PCH | ✅ 지원 | clang++ GCH 방식 |
| 공유 라이브러리(.dll) | ✅ 지원 | import lib(.dll.a) 별도 생성 없음 |
| 정적 라이브러리(.a) | ✅ 지원 | MinGW 관례 (.lib 아님) |
| UWP | ❌ 미지원 | MSVC 전용 |

---

## 8. C++ Modules 지원 설계

### 8-1. BMI 생성 및 참조

| 항목 | MSVC | Clang |
|------|------|-------|
| 확장자 | `.ifc` | `.pcm` |
| 생성 | `/interface /TP` | `--precompile -x c++-module` |
| 참조 | `/reference <n>=<f>.ifc` | `-fmodule-file=<n>=<f>.pcm` |
| 의존성 스캔 | `/scanDependencies out.json` | `clang-scan-deps -format=p1689` |
| 캐시 위치 | `.dotori-cache/bmi/` | `.dotori-cache/bmi/` |

### 8-2. 빌드 순서

의존성 스캔 결과(P1689)를 토폴로지 정렬하여 BMI 생성 순서를 확정합니다.
Unity Build는 Modules 소스 파일을 자동으로 제외합니다.

---

## 9. 컴파일러 플래그 매핑

### MSVC (`cl.exe`)

| 설정 | 플래그 |
|------|--------|
| `std = c++23` | `/std:c++latest` |
| `optimize = none/speed/size/full` | `/Od` / `/O2` / `/O1` / `/Ox /GL` |
| `debug-info = full/minimal/none` | `/Zi` / `/Z7` / (없음) |
| `runtime-link = static` | `/MT` (debug: `/MTd`) |
| `runtime-link = dynamic` | `/MD` (debug: `/MDd`) |
| `lto = true` | `/GL` + `/LTCG` |
| `warnings = all` | `/W4` |
| `warnings-as-errors = true` | `/WX` |
| PCH 생성/사용 | `/Yc` / `/Yu` |
| Module interface | `/interface /TP` |
| 헤더 의존성 | `/showIncludes` |
| UWP | `/ZW /EHsc` + `WindowsApp.lib` |

### clang-cl (Windows + MSVC SDK)

MSVC 플래그를 그대로 사용합니다 (§ MSVC 표 참조). 추가 사항:

| 설정 | 플래그 |
|------|--------|
| Windows SDK 헤더 | `-imsvc "<VcToolsDir>/include"` |
| Windows SDK ucrt 헤더 | `-imsvc "<WinSdkDir>/Include/<ver>/ucrt"` |
| Windows SDK um 헤더 | `-imsvc "<WinSdkDir>/Include/<ver>/um"` |
| Windows SDK shared 헤더 | `-imsvc "<WinSdkDir>/Include/<ver>/shared"` |
| Windows SDK lib (링커) | `/LIBPATH:"<WinSdkDir>/Lib/<ver>/um/<arch>"` |
| Windows SDK ucrt lib (링커) | `/LIBPATH:"<WinSdkDir>/Lib/<ver>/ucrt/<arch>"` |

### Clang (`clang++`)

| 설정 | 플래그 |
|------|--------|
| `std = c++23` | `-std=c++23` |
| `optimize = none/speed/size/full` | `-O0` / `-O2` / `-Os` / `-O3` |
| `debug-info = full/minimal/none` | `-g` / `-gline-tables-only` / (없음) |
| `stdlib = libc++` | `-stdlib=libc++` |
| `stdlib = libstdc++` | `-stdlib=libstdc++` |
| `runtime-link = static` (libc++) | `-static-libstdc++ -lc++abi` |
| `runtime-link = static` (libstdc++) | `-static-libgcc -static-libstdc++` |
| musl | `--target x86_64-unknown-linux-musl` |
| musl + static | 위 + `-static` |
| `lto = true` | `-flto` |
| PCH 생성/사용 | `-x c++-header` / `-include-pch` |
| Module BMI 생성 | `--precompile -x c++-module` |
| 헤더 의존성 | `-MD -MF <f>.d` |
| Apple SDK | `-isysroot $(xcrun --sdk <sdk> --show-sdk-path)` |
| Android sysroot | `--sysroot=$NDK/sysroot` |
| WASM bare | `--target wasm32-unknown-unknown --no-standard-libraries` |
| MinGW 크로스 | `--target x86_64-w64-mingw32 --sysroot=<mingw-sysroot>` |
| MinGW static runtime | `-static-libgcc -static-libstdc++` |

---

## 10. 아키텍처 설계

```
dotori (CLI)
│
├── ProjectLocator          — .dotori 파일 탐색 + 대화형 선택
│
├── DSL Parser              — .dotori 파싱
├── Project Model           — 조건 섹션 평탄화 (타겟 + Config 기준)
│   └── RuntimeEnforcer     — UWP/iOS/WASM 런타임 강제 규칙 적용
│
├── Dependency Resolver     — PubGrub (git/version 의존성)
│   ├── Git Fetcher
│   └── Path Resolver
│
├── Project DAG Builder     — path 의존성 기반 빌드 순서 결정
│   └── CycleDetector       — 순환 의존성 감지
│
├── Build Planner
│   ├── Glob Expander
│   ├── Module Scanner      — P1689 스캔
│   ├── Module Sorter       — BMI 토폴로지 정렬
│   ├── PCH Planner
│   └── Unity Batcher
│
├── Incremental Checker     — 해시 기반 증분 빌드
│
├── Build Executor
│   ├── Local Executor      — 로컬 병렬 실행
│   └── Remote Executor     — 분산 빌드 (Phase 2~3)
│
├── Toolchain
│   ├── ToolchainDetector
│   │   └── CrossCompileDetector  — MinGW/llvm-mingw 탐색 (비-Windows 호스트)
│   ├── MsvcDriver                — cl.exe / clang-cl 공통 (MSVC 플래그)
│   ├── ClangDriver               — clang++ / MinGW 크로스
│   └── EmscriptenDriver
│
├── Linker
│   ├── MsvcLinker
│   ├── LldLinker
│   └── AppleLinker
│
├── Package Manager
│   ├── DependencyResolver
│   ├── GitFetcher
│   ├── PathResolver
│   └── LockManager
│
└── Language Server (Phase 3)     — LSP 서버 (stdio transport)
    ├── DotoriLspServer           — LSP 요청/응답 라우팅
    ├── DiagnosticsProvider       — .dotori 구문/시맨틱 오류
    ├── CompletionProvider        — 키워드·경로·패키지명 자동완성
    ├── HoverProvider             — 키워드 설명 + 패키지 정보
    ├── DefinitionProvider        — path 의존성 → 대상 .dotori 이동
    └── CompileCommandsExporter   — compile_commands.json 생성
```

### IDE 확장 레이어 구조 (Phase 3)

LSP 서버(`Dotori.LanguageServer`)가 `.dotori` 편집 기능을 담당하고,
각 IDE 플러그인은 빌드/실행/디버그 통합을 네이티브 API로 담당합니다.

```
┌─────────────────────────────────────────────────────┐
│  Dotori.LanguageServer  (공통, stdio LSP)            │
│  • .dotori 구문 강조 / 자동완성 / 진단               │
│  • 정의로 이동 (path 의존성)                         │
│  • compile_commands.json 생성                        │
└──────────────┬──────────────────────────────────────┘
               │  LSP (stdio / JSON-RPC 2.0)
         ┌─────┴─────┐
         ▼           ▼
    ┌────────┐  ┌────────┐
    │ VSCode │  │  Zed   │
    │ ext.   │  │  ext.  │
    │(TypeSc)│  │(Rust)  │
    ├────────┤  ├────────┤
    │ Task   │  │ Task   │
    │Provider│  │Provider│
    │ Launch │  │        │
    │ Config │  │        │
    └────────┘  └────────┘
```

---

## 11. 구현 작업 목록

### Phase 1-A: 프로젝트 셋업

- [x] C# .NET 8 솔루션 구조
  - `Dotori.Cli/`, `Dotori.Core/`, `Dotori.PackageManager/`, `Dotori.Tests/`
- [x] NativeAOT 배포 설정 (Reflection/dynamic 사용 금지)
- [x] CI: Windows (amd64), macOS (arm64), Linux (amd64)

---

### Phase 1-B: DSL 파서

- [x] 렉서 (소스 위치 기록, `#` 줄 주석, `(*...*)` 블록 주석 하위호환)
- [x] 파서 — `project` / `package` 블록, 한 파일에 둘 다 허용
- [x] 조건 섹션 병합
  - 조건 구체성 우선순위: `[windows.release]` > `[windows]` > 공통
  - 런타임 강제 규칙:
    - `[uwp]` → `runtime-link = dynamic` 강제
    - `[ios]` / `[tvos]` / `[watchos]` → `runtime-link = static` 강제
    - `[wasm]` → `runtime-link = static` 강제 (설정 무시)
- [x] 파서 단위 테스트 (`tests/fixtures/*.dotori`)

---

### Phase 1-C: 프로젝트 탐색 (`ProjectLocator`)

- [x] 탐색 순서 구현
  1. `--project <path>` 옵션
  2. 현재 디렉토리 `.dotori`
  3. 상위 디렉토리 재귀 탐색 (git root까지)
  4. 하위 디렉토리 탐색 후 목록 수집
  5. 하나 → 자동 선택 / 둘 이상 → 대화형 선택 / 없음 → 오류
- [x] 대화형 선택 프롬프트 (번호 선택 / Enter = 전체)
- [x] `--all` 플래그 (프롬프트 없이 전체 빌드)

---

### Phase 1-D: 프로젝트 DAG 빌더

- [x] `path` 의존성을 재귀적으로 탐색하여 전체 프로젝트 그래프 구성
- [x] 토폴로지 정렬 → 빌드 순서 결정
- [x] 순환 의존성 감지 + 명확한 오류 출력
- [x] DAG 병렬화: 의존 관계 없는 프로젝트 동시 빌드

---

### Phase 1-E: 툴체인 감지

| 타겟 | 탐색 방법 |
|------|---------|
| Windows MSVC | `vswhere.exe`, `VCINSTALLDIR` |
| Windows clang-cl + MSVC SDK | PATH `clang-cl.exe` + `vswhere.exe`로 SDK 확인 |
| Windows Clang (SDK 없음) | PATH `clang++`, `lld-link` |
| Linux Clang | PATH `clang++` |
| macOS Clang | `xcrun --find clang++` |
| iOS/tvOS/watchOS | `xcrun --sdk <sdk> --find clang++` |
| Android | `ANDROID_NDK_HOME` / `ANDROID_HOME` |
| WASM Emscripten | `EMSDK_PATH` / PATH `emcc` |
| WASM bare | PATH `clang++` |
| **Windows (크로스, llvm-mingw)** | PATH `<triple>-clang++` |
| **Windows (크로스, clang+MinGW)** | PATH `clang++` + `MINGW_SYSROOT` / Homebrew / `/usr/<triple>` |

---

### Phase 1-F: 컴파일러 드라이버 + 빌드 플래너

- [x] `MsvcDriver`, `ClangDriver`, `EmscriptenDriver`
- [x] `MsvcLinker`, `LldLinker`, `AppleLinker`
- [x] Glob 확장기
- [x] DAG 생성 (파일 단위: `.cpp` → `.obj`, `.cppm` → `.bmi`)
- [x] C++ Modules 스캔 + BMI 순서 결정
- [x] PCH 플래너 (Modules와 PCH 동시 사용 시 경고)
- [x] Unity Batcher (Modules 소스 자동 제외)
- [x] 증분 빌드 해시 DB (`.dotori-cache/hashes.db`)
- [x] 병렬 빌드 (`Channel<BuildJob>`)
- [x] 단일 파일 빌드 (`--file`)
  - 지정 파일이 프로젝트 `sources` 목록에 포함되어 있는지 검증
  - **Unity Build 꺼진 경우**
    - 해당 `.cpp` 만 컴파일 → `.obj` 생성
    - `--no-link` 없으면: 나머지 기존 `.obj` + 새 `.obj` 링크
    - `--no-link` 있으면: `.obj` 생성 후 종료
  - **Unity Build 켜진 경우**
    - 기본(`--file`): 해당 파일이 속한 unity batch `.cpp` 를 컴파일 → `.obj` 생성 후 링크
    - `--no-link`: unity batch `.cpp` 만 컴파일 후 종료
    - `--no-unity`: unity batch 무시, 지정 파일 단독 직접 컴파일 (링크 동작은 동일)
  - **C++ Modules 파일 (`.cppm`/`.ixx`) 지정 시**
    - 해당 파일의 BMI 만 재생성 (`--no-link` 암묵적으로 적용)
    - 의존 BMI가 없으면 선행 BMI도 자동으로 함께 빌드
  - **PCH가 있는 경우**
    - PCH `.obj` 가 없거나 구버전이면 PCH 먼저 재빌드 후 진행

---

### Phase 1-G: 패키지 매니저

- [x] PubGrub 의존성 해결기
- [x] Git Fetcher (`~/.dotori/packages/<n>/<ver>/`)
- [x] Lock 파일 관리 (`.dotori.lock`)

---

### Phase 1-H: CLI

```bash
# 빌드 / 실행
dotori build                             # 현재 디렉토리 탐색
dotori build --project ./lib             # 경로 직접 지정
dotori build --all                       # 전체 빌드 (프롬프트 없음)
dotori build --release
dotori build --target ios-arm64
dotori build --target android-arm64
dotori build --target wasm32-emscripten
dotori build --target wasm32-bare
dotori build --compiler clang
dotori build --runtime-link dynamic
dotori build --libc musl
dotori build --stdlib libc++
dotori build --jobs 8
dotori build --no-modules

# 단일 파일 빌드
dotori build --file src/foo.cpp          # .obj 생성 후 전체 링크
dotori build --file src/foo.cpp --no-link  # .obj 만 생성 (링크 없음)

# Unity Build가 켜진 경우:
#   --file src/foo.cpp          → foo.cpp 가 포함된 unity batch 파일을 빌드 후 전체 링크
#   --file src/foo.cpp --no-link → unity batch 파일만 빌드 (.obj 만 생성)
#   --file src/foo.cpp --no-unity → unity build 무시, foo.cpp 단독으로 직접 컴파일

dotori run
dotori run --project ./app
dotori run --release
dotori run -- <args>

dotori test
dotori test --filter "MyTest*"

# 정리
dotori clean
dotori clean --all

# 포매팅
dotori format                            # 현재 디렉토리 탐색
dotori format .dotori                    # 파일 직접 지정
dotori format --project ./lib            # 프로젝트 경로 지정
dotori format --check                    # 포매팅 여부만 확인 (CI 사용 가능)
dotori format --stdout .dotori           # stdout으로 출력

# 패키지 관리
dotori add <n>
dotori add <n>@1.2.0
dotori add <url> --git
dotori add <path> --path
dotori remove <n>
dotori update
dotori update <n>

# 정보 / 진단
dotori list
dotori graph                             # 프로젝트 DAG + 의존성 그래프 출력
dotori info
dotori check                             # .dotori 유효성 검사
dotori targets                           # 사용 가능한 타겟 목록
dotori toolchain                         # 감지된 툴체인 정보
dotori --version
```

---

### Phase 1-I: Hello World 검증

- [x] `dotori build` + `dotori run` (현재 디렉토리 탐색 UX 검증)
- [x] 멀티 프로젝트 DAG 빌드 검증 (`app` → `lib` → `core` 구조)
- [x] 순환 의존성 오류 메시지 검증
- [ ] 검증 환경
  - `windows-x64` (MSVC)
  - `windows-x64` (Clang + lld-link, MSVC 없음)
  - `linux-x64` (glibc + libstdc++ + dynamic)
  - `linux-x64` (musl + static → 완전 정적 바이너리)
  - [x] `macos-arm64`
  - `ios-arm64` (macOS에서 크로스)
  - `android-arm64`
  - `wasm32-emscripten`
  - `wasm32-bare`
- [x] C++ Modules 검증 (Clang, macos-arm64)
- [x] git 의존성 + path 의존성 검증
- [x] 단일 파일 빌드 검증
  - [x] `--file` + 링크 / `--file --no-link` / `--file --no-unity` 세 케이스
  - [x] Unity Build 켜진 프로젝트에서 `--file` → 올바른 unity batch 파일이 선택되는지 확인
  - [x] `.cppm` 지정 시 BMI만 재생성되는지 확인
- [x] PCH 빌드 검증 (macos-arm64)
- [x] Unity Build 검증 (macos-arm64, exclude 패턴 동작 확인)

---

### Phase 1-J: 출력 디렉토리 분리 + 빌드 스크립트 + 모듈 Export Map

#### 1. 출력 디렉토리 분리 (`output { }`)

- [x] DSL 파서: `output { }` 블록 파싱
- [x] `FlatProjectModel` 에 `OutputConfig` 추가
- [x] `BuildPlanner` / `BuildCommand` 에 링크 후 복사 단계 추가
- [x] `dotori clean` 시 output 경로 복사본도 삭제

#### 2. 빌드 전/후 스크립트 (`pre-build { }` / `post-build { }`)

- [x] DSL 파서: `pre-build { }` / `post-build { }` 블록 파싱
- [x] `FlatProjectModel` 에 `PreBuildCommands` / `PostBuildCommands` 추가
- [x] `BuildCommand` / `RunCommand` 에 스크립트 실행 단계 삽입
- [x] stdout/stderr → dotori 콘솔 출력으로 연결

#### 3. C++ Modules Export Map (자동 생성)

생성 파일: `.dotori-cache/obj/<target>-<config>/bmi/module-map.json` (P1689 기반, `modules { export-map = true }` 로 제어)

- [x] DSL 파서: `modules { export-map = true/false }` 파싱
- [x] `FlatProjectModel` → `ModuleExportMap` 필드 추가
- [x] `BuildPlanner.WriteModuleMap()`: BMI 생성 후 module-map.json 기록
- [x] `dotori clean` 시 module-map.json 삭제

---

### Phase 1-K: 사용자 정의 컴파일러/링커 플래그 (`compile-flags` / `link-flags`)

DSL 속성으로 제어되지 않는 컴파일러·링커 옵션을 직접 지정하는 기능.
기존 조건 블록(`[msvc]`, `[clang]`, `[windows]`, `[release]` 등)과 조합해 컴파일러별로 다른 값을 줄 수 있음.

구현 대상:
- [x] DSL 파서: `compile-flags { }` / `link-flags { }` 블록 파싱
- [x] `FlatProjectModel` 에 `CompileFlags` / `LinkFlags` (`List<string>`) 추가
  - 조건 병합 시 리스트를 **누적(append)** 방식으로 병합 (덮어쓰기 아님)
- [x] `MsvcDriver` / `ClangDriver` / `EmscriptenDriver` 에 compile-flags 주입
  - dotori 생성 플래그 **뒤에** 추가
- [x] `MsvcLinker` / `LldLinker` / `AppleLinker` 에 link-flags 주입
  - dotori 생성 플래그 **뒤에** 추가
- [x] `dotori check`: 조건 블록 없이 `compile-flags`/`link-flags` 사용 시 이식성 경고 출력
  - 메시지: `warning: compile-flags used without a compiler condition (e.g. [msvc], [clang]) — may reduce portability`
- [x] 단위 테스트
  - DSL 파서: 조건 블록 내/외 `compile-flags` / `link-flags` 파싱
  - 플래그 병합: `[msvc]` 조건 + 공통 블록의 누적 동작 검증
  - 드라이버 플래그 주입: 생성된 컴파일 커맨드에 사용자 플래그가 포함되는지 검증

---

### Phase 1-L: 환경변수 보간 (`${VAR}`)

DSL 문자열 값 안에서 `${VAR}` 형태로 환경변수를 참조하는 기능.
빌드 시작 시(`ProjectFlattener.Flatten()`) 확장됩니다.

구현 대상:
- [x] `EnvExpander.Expand(string)` 유틸리티 클래스 구현
  - `${VAR}` → 환경변수 값 (없으면 빈 문자열)
  - 닫히지 않은 `${` → 리터럴 유지
- [x] `ProjectFlattener.ApplyItems()` 에서 모든 사용자 작성 문자열에 적용
  - sources/modules glob, headers 경로, defines, links, frameworks
  - compile-flags, link-flags, emscripten-flags
  - pre-build/post-build 명령어, output 경로, pch 경로
  - unity-build.exclude glob, dependencies.path/git/tag/commit/version
- [x] 단위 테스트
  - `${VAR}` 확장, 미정의 변수 빈 문자열, 닫히지 않은 `${` 리터럴
  - `FlatProjectModel` 에 확장된 값이 들어오는지 검증

---

---

### Phase 1-M: Windows Clang SDK 연동 + MinGW 크로스 컴파일

#### 1. clang-cl + Windows SDK 연동 (Windows 호스트)

- [x] `ToolchainInfo`에 `IsClangCl` 계산 속성 추가
  - `Kind == Msvc && Path.GetFileNameWithoutExtension(CompilerPath) == "clang-cl"`
- [x] `ToolchainDetector.TryGetMsvcPaths(arch)` 헬퍼 메서드 추출
  - 기존 `TryFindMsvc()` 내부 로직에서 MSVC 경로 수집 부분 분리
- [x] `TryFindClangWindows()` 수정
  - `clang-cl.exe` 발견 + MSVC SDK 존재 → `Kind = Msvc`, `CompilerPath = clang-cl.exe`, `LinkerPath = lld-link.exe`, `Msvc = <SDK 경로>` 반환
  - `clang-cl.exe` 발견 + SDK 없음 → 기존 동작 유지 (`Kind = Clang`)
- [x] `MsvcDriver.CompileFlags()` 수정
  - `toolchain.IsClangCl == true` 일 때 `-imsvc` 플래그로 Windows SDK 헤더 경로 명시 추가
    - `-imsvc "<VcToolsDir>/include"`
    - `-imsvc "<WinSdkDir>/Include/<ver>/ucrt"`
    - `-imsvc "<WinSdkDir>/Include/<ver>/um"`
    - `-imsvc "<WinSdkDir>/Include/<ver>/shared"`
  - `cl.exe` 사용 시에는 추가하지 않음 (cl.exe가 자동 처리)
- [x] 단위 테스트: `IsClangCl == true`일 때 `-imsvc` 플래그가 포함되는지 검증

#### 2. MinGW 크로스 컴파일 (비-Windows 호스트 → Windows 타겟)

- [x] `ToolchainInfo`에 `IsMinGW` 계산 속성 추가
  - `Kind == Clang && TargetTriple.Contains("mingw")`
- [x] `ToolchainDetector.DetectWindows()` 수정
  - Windows 호스트가 아닌 경우 → `DetectWindowsCross(arch)` 호출
- [x] `ToolchainDetector.DetectWindowsCross(string arch)` 추가
  1. `<triple>-clang++` PATH 탐색 (llvm-mingw)
  2. PATH `clang++` + `FindMinGWSysroot(arch)` 탐색
  3. 발견 시 `ToolchainInfo { Kind = Clang, TargetTriple = "<triple>-w64-mingw32", SysRoot = <sysroot> }` 반환
- [x] `ToolchainDetector.FindMinGWSysroot(string arch)` 추가
  - `MINGW_SYSROOT` 환경변수 → Homebrew 경로 → Linux `/usr/<triple>` 순으로 탐색
- [x] `BuildPlanner.Link.cs` 수정
  - `GetOutputName()`: MinGW (`toolchain.IsMinGW`) + 정적 라이브러리 → `lib<name>.a` (`.lib` 아님)
  - `FindAr()`: 인스턴스 메서드로 변경, MinGW일 때 `<triple>-ar` 우선 탐색
- [x] 단위 테스트
  - `DetectWindowsCross()`: MinGW sysroot 탐색 로직 검증
  - `IsMinGW == true`일 때 정적 라이브러리 출력명이 `.a`인지 검증

---

### Phase 2: 분산 빌드 ✅ 완료

---

### Phase 3: IDE 확장

#### 설계 원칙

- **공통 LSP 서버** (`Dotori.LanguageServer`): `.dotori` 파일 편집 기능을 하나의 서버로 구현
- **얇은 IDE 래퍼**: 각 IDE 플러그인은 빌드/실행/디버그 통합만 담당 (언어 기능은 LSP 서버에 위임)
- **`compile_commands.json` 생성**: C++ 소스 파일에 대한 IntelliSense는 clangd에 위임 (LSP 서버 직접 처리 안 함)

---

#### Phase 3-A: Language Server (`Dotori.LanguageServer`)

프로젝트 구조:
- `Dotori.LanguageServer/` — 독립 실행형 LSP 서버 (NativeAOT 단일 바이너리)
- `dotori lsp` CLI 서브커맨드로도 시작 가능 (stdio transport)

**통신**
- 전송: stdio (JSON-RPC 2.0, LSP 3.17 사양)
- 프로세스 관리: 각 IDE 플러그인이 `dotori lsp` 프로세스를 생성/종료

**구현 대상**

- [x] LSP 서버 기반 구조 (`DotoriLspServer`)
  - `initialize` / `initialized` / `shutdown` / `exit` 처리
  - `textDocument/didOpen` / `didChange` / `didClose` — 파일 동기화
  - 진단 결과를 `textDocument/publishDiagnostics` 로 push

- [x] 진단 (`DiagnosticsProvider`)
  - DSL 파서 오류 → LSP `Diagnostic` 변환 (소스 위치 포함)
  - 시맨틱 검사:
    - 알 수 없는 키워드/속성 값 경고
    - `path` 의존성 대상 `.dotori` 파일 존재 여부 확인
    - `type` 중복 선언 오류
    - `compile-flags`/`link-flags` 조건 블록 없이 사용 시 이식성 경고

- [x] 자동완성 (`CompletionProvider`, `textDocument/completion`)
  - DSL 최상위 키워드: `project`, `package`
  - `project_prop` 키워드: `type`, `std`, `optimize`, `debug-info`, `runtime-link`, …
  - 열거형 값: `c++17`/`c++20`/`c++23`, `executable`/`static-library`/…
  - 조건 키워드: `windows`, `linux`, `macos`, `debug`, `release`, `msvc`, `clang`, …
  - `path =` 값: 상대 경로 파일시스템 완성
  - `include`/`exclude` glob: 상대 경로 완성
  - `headers { public/private "…" }`: 디렉토리 경로 완성

- [x] 호버 (`HoverProvider`, `textDocument/hover`)
  - DSL 키워드에 대한 한줄 설명 (Markdown)
  - 열거형 값 설명 (예: `musl` → "완전 정적 바이너리 가능, Linux 전용")
  - `path` 의존성 hover → 대상 프로젝트 이름 + 타입 표시

- [x] 정의로 이동 (`DefinitionProvider`, `textDocument/definition`)
  - `path = "../lib"` → `../lib/.dotori` 파일 열기

- [x] `compile_commands.json` 생성 (`CompileCommandsExporter`)
  - `dotori generate-compile-commands` CLI 명령으로 호출
  - 현재 호스트 환경 기준 타겟 자동 선택 (macOS → `macos-arm64` 등)
  - `--target <id>` 옵션으로 명시 가능
  - 출력 위치: 프로젝트 루트 `compile_commands.json`
  - 멀티 프로젝트: 각 프로젝트 항목을 하나의 파일에 병합
  - IDE 플러그인이 프로젝트 열기 시 자동 실행 (각 플러그인 담당)

- [x] 단위 테스트
  - LSP 요청/응답 직렬화 검증
  - `DiagnosticsProvider`: 파서 오류 → `Diagnostic` 변환
  - `CompletionProvider`: 각 문맥별 완성 항목 검증
  - `CompileCommandsExporter`: 생성된 JSON 구조 검증

---

#### Phase 3-B: VSCode 확장 (`dotori-vscode`)

언어: TypeScript
배포: VS Code Marketplace

**구현 대상**

- [ ] LSP 클라이언트
  - `dotori lsp` 프로세스 시작 (stdio)
  - `.dotori` 파일에 자동 연결
  - 서버 재시작 명령: `dotori: Restart Language Server`

- [ ] TextMate 문법 (`dotori.tmLanguage.json`)
  - 구문 강조: 키워드, 문자열, 주석 `#`, 조건 블록 `[...]`
  - LSP 없이도 기본 강조 동작

- [ ] 빌드 Task Provider (`tasks.json` 통합)
  - `dotori: Build` — `dotori build`
  - `dotori: Build (Release)` — `dotori build --release`
  - `dotori: Clean` — `dotori clean`
  - `dotori: Run` — `dotori run`
  - `dotori: Run (Release)` — `dotori run --release`
  - 문제 매처(problemMatcher): 컴파일러 오류 → 에디터 오류 연결

- [ ] 디버그 Launch Configuration Provider
  - `launch.json` 자동 생성 템플릿 제공
  - `dotori build --release` 선행 빌드 후 실행 파일 자동 감지

- [ ] `compile_commands.json` 자동 갱신
  - `.dotori` 파일 저장 시 `dotori generate-compile-commands` 실행
  - `clangd` 확장과 연동 (별도 설치 권장 메시지)

- [ ] 설정 항목 (`settings.json`)
  - `dotori.lspPath`: LSP 바이너리 경로 (기본: PATH 탐색)
  - `dotori.defaultTarget`: 기본 빌드 타겟 (기본: 호스트 자동 감지)
  - `dotori.autoGenerateCompileCommands`: 저장 시 자동 갱신 여부

---

#### Phase 3-C: Zed 확장 (`dotori-zed`)

언어: Rust
배포: Zed Extension Registry

Zed는 LSP를 네이티브로 지원하므로 별도 LSP 클라이언트 구현 없이 선언으로 연결합니다.

**구현 대상**

- [ ] LSP 서버 등록 (`extension.toml`)
  - `.dotori` 파일에 `dotori lsp` 연결
  - 언어 정의: 파일 확장자 `.dotori`, 줄 주석 `#`, 블록 주석 `(* ... *)` (하위호환)

- [ ] 구문 강조 (Tree-sitter 문법)
  - `.dotori` DSL용 Tree-sitter 문법 파일 작성
  - 키워드, 문자열, 주석, 조건 블록, 식별자 하이라이팅

- [ ] 빌드 Task 정의
  - `dotori build`, `dotori run`, `dotori clean` 태스크
  - Zed 태스크 실행기와 통합

- [ ] `compile_commands.json` 자동 갱신
  - `.dotori` 파일 저장 훅 → `dotori generate-compile-commands` 실행

---

#### Phase 3 CLI 추가 명령

```bash
# Language Server 시작 (IDE 플러그인이 내부적으로 호출)
dotori lsp                              # stdio transport (LSP 3.17)
dotori lsp --log-file /tmp/dotori.log   # 디버그 로그 출력

# compile_commands.json 생성
dotori generate-compile-commands                    # 호스트 기본 타겟
dotori generate-compile-commands --target linux-x64 # 타겟 명시
dotori generate-compile-commands --output ./         # 출력 경로 지정
```

---

### Phase 4: 레지스트리 서버

#### 패키지 레지스트리 (`dotori-registry`)

- [x] REST API 서버 (ASP.NET Core)
  - `GET /packages/{name}` — 패키지 메타데이터 조회
  - `GET /packages/{name}/{version}` — 특정 버전 정보
  - `GET /packages/{name}/{version}/download` — 소스 아카이브 다운로드
  - `POST /packages/publish` — 패키지 배포 (인증 필요)
  - `GET /packages/search?q=...` — 패키지 검색
- [x] 패키지 저장소 백엔드 (파일시스템 / S3 호환 오브젝트 스토리지)
- [x] 패키지 무결성 검증 (SHA-256 해시 + 서명)
- [x] 사용자 인증 / API 토큰 관리
- [x] 패키지 버전 yanking (취약 버전 비활성화)
- [x] `dotori login` / `dotori logout` CLI 명령 연동
- [x] `dotori publish` CLI 명령 — `package { }` 블록 기반 자동 배포
- [ ] Docker / Kubernetes 배포 구성
- [x] 미러 / 프록시 레지스트리 지원 (사내 레지스트리 구축)

---

## 12. 캐시 디렉토리 구조

```
프로젝트 로컬:
.dotori-cache/
├── hashes.db
├── bmi/
│   ├── MyLib.ifc          # MSVC
│   └── MyLib.pcm          # Clang
├── unity/
│   └── unity_0.cpp
└── obj/
    ├── windows-x64-debug/
    ├── windows-x64-release/
    ├── linux-x64-glibc-static-debug/
    ├── linux-x64-musl-static-release/
    ├── android-arm64-debug/
    └── wasm32-emscripten-release/

전역:
~/.dotori/
└── packages/
    ├── fmt/10.2.0/
    └── spdlog/1.13.0/
```

---

## 14. 미결정 사항

| 번호 | 항목 | 잠정 처리 | 결정 시 영향 범위 |
|------|------|-----------|-----------------|
| M-1 | UWP 추가 WinRT 지원 범위 | 최소 `/ZW` + WinRT 링크만 | MSVC 드라이버 |
| M-2 | iOS xcframework (FAT 바이너리) | Phase 2 이후 | Apple 링커 |
| M-3 | 분산 빌드 프로토콜 | gRPC vs HTTP REST | Phase 2 서버 |
| M-4 | 레지스트리 서버 | Phase 3 이후 | 패키지 매니저 |
| M-5 | Zed Tree-sitter 문법 완성도 | 기본 하이라이팅만 구현 | Zed 확장 |

