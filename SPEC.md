# dotori — C++ 빌드 시스템 + 패키지 매니저 작업내역서

> **작성일**: 2026-03-08  
> **버전**: v1  
> **구현 언어**: C# (.NET 8+, NativeAOT 배포)  
> **목표**: Cargo 수준의 UX를 C++에 제공하는 독립형 빌드 시스템 + 패키지 매니저  
> **외부 호환성**: 의도적으로 제외 (vcpkg/Conan/CMake 브리지 없음)  
> **컴파일러 지원**: MSVC, Clang

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
(* app/.dotori *)
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
        (* runtime-link = dynamic 강제 적용 *)
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
        (* runtime-link = static 강제 적용 *)
    }

    [tvos] {
        sources    { include "src/platform/apple/**/*.cpp" }
        defines    { "PLATFORM_TVOS" }
        frameworks { "Foundation" "UIKit" "Metal" }
        tvos-min   = "15.0"
    }

    [watchos] {
        sources    { include "src/platform/apple/**/*.cpp" }
        defines    { "PLATFORM_WATCHOS" }
        frameworks { "Foundation" "WatchKit" }
        watchos-min = "8.0"
    }

    [wasm] {
        sources { include "src/platform/wasm/**/*.cpp" }
        defines { "PLATFORM_WASM" }
        (* runtime-link 설정 무시, 항상 static *)
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
        my-lib = { path = "../lib" }     (* 로컬 프로젝트 — 빌드 순서 자동 결정 *)
        fmt    = "10.2.0"                (* 패키지 매니저 *)
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
}
```

```
(* lib/.dotori — project + package 함께 작성 *)
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

```
$ dotori build
No .dotori file found in current directory.
Found 3 projects:

  [1] app/MyApp        (executable)    app/.dotori
  [2] lib/MyLib        (static-library) lib/.dotori
  [3] tools/MyTool     (executable)    tools/.dotori

Select project [1-3] or press Enter to build all: _
```

- 번호 입력 → 해당 프로젝트만 빌드
- Enter → 발견된 모든 프로젝트를 의존성 순서에 따라 빌드
- `--all` 플래그 → 프롬프트 없이 전체 빌드

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

### 4-2. DAG 구성 예시

```
app/.dotori       → depends on: lib, core
lib/.dotori       → depends on: core
core/.dotori      → depends on: (없음)
tools/.dotori     → depends on: core

빌드 순서: core → lib → app
                → tools  (core 완료 후 lib와 tools는 병렬 빌드 가능)
```

### 4-3. 순환 의존성 오류

```
$ dotori build
Error: Circular dependency detected:
  app → lib → utils → app
```

### 4-4. 전체 빌드 시 독립 프로젝트 병렬화

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

```ebnf
file            ::= { top_decl }
top_decl        ::= project_decl | package_decl

ident           ::= [a-zA-Z_][a-zA-Z0-9_\-]*
string          ::= '"' { char } '"'
integer         ::= [0-9]+
bool_val        ::= "true" | "false"
version_str     ::= string
path_glob       ::= string

dep_value       ::= version_str
                  | "{" dep_option { "," dep_option } "}"
dep_option      ::= "git"     "=" string
                  | "tag"     "=" string
                  | "commit"  "=" string
                  | "path"    "=" string      (* 빌드 순서 DAG에 포함 *)
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
                  | dependencies_block
                  | pch_block
                  | unity_build_block
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
modules_block   ::= "modules"    "{" { source_item } "}"
source_item     ::= ( "include" | "exclude" ) path_glob

headers_block   ::= "headers"    "{" { header_item } "}"
header_item     ::= ( "public" | "private" ) string

defines_block   ::= "defines"    "{" { string | ident } "}"
links_block     ::= "links"      "{" { string | ident } "}"
frameworks_block::= "frameworks" "{" { string | ident } "}"

dependencies_block ::= "dependencies" "{" { dep_item } "}"
dep_item        ::= ident "=" dep_value

pch_block       ::= "pch" "{" { pch_prop } "}"
pch_prop        ::= ( "header" | "source" ) "=" string
                  | "modules" "=" bool_val

unity_build_block ::= "unity-build" "{" { unity_prop } "}"
unity_prop      ::= "enabled"    "=" bool_val
                  | "batch-size" "=" integer
                  | "exclude"    "{" { path_glob } "}"

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

```
컴파일러 선택:
  auto   → MSVC 우선, 없으면 Clang
  msvc   → cl.exe (vswhere.exe로 탐색)
  clang  → MSVC 있으면 clang-cl (MSVC STL)
           MSVC 없으면 clang++ + lld-link (libc++)

런타임 링크:
  static  → /MT (debug: /MTd)
  dynamic → /MD (debug: /MDd)
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
│   ├── MsvcDriver
│   ├── ClangDriver
│   └── EmscriptenDriver
│
├── Linker
│   ├── MsvcLinker
│   ├── LldLinker
│   └── AppleLinker
│
└── Package Manager
    ├── DependencyResolver
    ├── GitFetcher
    ├── PathResolver
    └── LockManager
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

- [x] 렉서 (소스 위치 기록, `(*...*)` 주석)
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
| Windows Clang | PATH `clang++`, LLVM 레지스트리 |
| Linux Clang | PATH `clang++` |
| macOS Clang | `xcrun --find clang++` |
| iOS/tvOS/watchOS | `xcrun --sdk <sdk> --find clang++` |
| Android | `ANDROID_NDK_HOME` / `ANDROID_HOME` |
| WASM Emscripten | `EMSDK_PATH` / PATH `emcc` |
| WASM bare | PATH `clang++` |

---

### Phase 1-F: 컴파일러 드라이버 + 빌드 플래너

- [x] `MsvcDriver`, `ClangDriver`, `EmscriptenDriver`
- [ ] `MsvcLinker`, `LldLinker`, `AppleLinker`
- [x] Glob 확장기
- [x] DAG 생성 (파일 단위: `.cpp` → `.obj`, `.cppm` → `.bmi`)
- [ ] C++ Modules 스캔 + BMI 순서 결정
- [ ] PCH 플래너 (Modules와 PCH 동시 사용 시 경고)
- [x] Unity Batcher (Modules 소스 자동 제외)
- [x] 증분 빌드 해시 DB (`.dotori-cache/hashes.db`)
- [x] 병렬 빌드 (`Channel<BuildJob>`)
- [ ] 단일 파일 빌드 (`--file`)
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

- [ ] PubGrub 의존성 해결기
- [ ] Git Fetcher (`~/.dotori/packages/<n>/<ver>/`)
- [ ] Lock 파일 관리 (`.dotori.lock`)

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

- [ ] `dotori build` + `dotori run` (현재 디렉토리 탐색 UX 검증)
- [ ] 멀티 프로젝트 DAG 빌드 검증 (`app` → `lib` → `core` 구조)
- [ ] 순환 의존성 오류 메시지 검증
- [ ] 검증 환경
  - `windows-x64` (MSVC)
  - `windows-x64` (Clang + lld-link, MSVC 없음)
  - `linux-x64` (glibc + libstdc++ + dynamic)
  - `linux-x64` (musl + static → 완전 정적 바이너리)
  - `macos-arm64`
  - `ios-arm64` (macOS에서 크로스)
  - `android-arm64`
  - `wasm32-emscripten`
  - `wasm32-bare`
- [ ] C++ Modules 검증 (MSVC + Clang)
- [ ] git 의존성 + path 의존성 검증
- [ ] 단일 파일 빌드 검증
  - `--file` + 링크 / `--file --no-link` / `--file --no-unity` 세 케이스
  - Unity Build 켜진 프로젝트에서 `--file` → 올바른 unity batch 파일이 선택되는지 확인
  - `.cppm` 지정 시 BMI만 재생성되는지 확인

---

### Phase 2: 분산 빌드

- [ ] `RemoteExecutor` 클라이언트
- [ ] `BuildServer` (Coordinator, ASP.NET Core)
- [ ] `WorkerAgent`
- [ ] 원격 빌드 캐시
- [ ] 서버 연결 실패 시 로컬 폴백
- [ ] Docker / Kubernetes 배포

---

## 12. 디렉토리 구조

```
dotori/
├── Dotori.slnx
├── src/
│   ├── Dotori.Cli/
│   │   ├── Program.cs
│   │   └── Commands/
│   │       ├── BuildCommand.cs
│   │       ├── NewCommand.cs
│   │       ├── RunCommand.cs
│   │       ├── TestCommand.cs
│   │       └── PackageCommands.cs
│   │
│   ├── Dotori.Core/
│   │   ├── Parsing/
│   │   │   ├── Lexer.cs
│   │   │   ├── Parser.cs
│   │   │   └── Ast.cs
│   │   ├── Model/
│   │   │   ├── ProjectModel.cs
│   │   │   ├── PackageModel.cs
│   │   │   ├── TargetModel.cs
│   │   │   └── RuntimeEnforcer.cs
│   │   ├── Location/
│   │   │   └── ProjectLocator.cs       ← .dotori 탐색 + 대화형 선택
│   │   ├── Graph/
│   │   │   ├── ProjectDagBuilder.cs    ← path 의존성 DAG
│   │   │   └── CycleDetector.cs
│   │   ├── Toolchain/
│   │   │   ├── ToolchainDetector.cs
│   │   │   ├── MsvcDriver.cs
│   │   │   ├── ClangDriver.cs
│   │   │   └── EmscriptenDriver.cs
│   │   ├── Linker/
│   │   │   ├── MsvcLinker.cs
│   │   │   ├── LldLinker.cs
│   │   │   └── AppleLinker.cs
│   │   ├── Build/
│   │   │   ├── BuildPlanner.cs
│   │   │   ├── GlobExpander.cs
│   │   │   ├── ModuleScanner.cs
│   │   │   ├── ModuleSorter.cs
│   │   │   ├── PchPlanner.cs
│   │   │   ├── UnityBatcher.cs
│   │   │   └── IncrementalChecker.cs
│   │   └── Executor/
│   │       ├── LocalExecutor.cs
│   │       └── RemoteExecutor.cs       ← Phase 2
│   │
│   ├── Dotori.PackageManager/
│   │   ├── DependencyResolver.cs
│   │   ├── GitFetcher.cs
│   │   ├── PathResolver.cs
│   │   └── LockManager.cs
│   │
│   ├── Dotori.BuildServer/             ← Phase 2
│   └── Dotori.Worker/                  ← Phase 2
│
└── tests/
    ├── Dotori.Tests.Parsing/
    │   └── fixtures/
    ├── Dotori.Tests.Graph/
    ├── Dotori.Tests.Build/
    └── Dotori.Tests.PackageManager/
```

---

## 13. 캐시 디렉토리 구조

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
| M-5 | VS Code 확장 | Phase 3 이후 | 에디터 통합 |

---

## 15. 작업 우선순위 및 예상 순서

```
Phase 1 — 로컬 빌드
1.  [ ] 프로젝트 셋업 + NativeAOT 검증                  (1~2일)
2.  [ ] DSL 파서 (project + package, 조건 섹션)         (3~5일)
3.  [ ] ProjectLocator (탐색 + 대화형 선택)             (2~3일)
4.  [ ] ProjectDagBuilder (path 의존성 DAG)             (2~3일)
5.  [ ] 툴체인 감지 (Windows/Linux/macOS)               (2~3일)
6.  [ ] Glob 확장기                                     (1~2일)
7.  [ ] MSVC 드라이버 (windows-x64)                    (3~5일)
8.  [ ] Clang 드라이버 (linux-x64)                     (2~3일)
9.  [ ] 빌드 그래프 DAG + 병렬 빌드                    (3~5일)
10. [ ] Hello World — windows-x64, linux-x64, macos-arm64
                                                   (1~2일)
11. [ ] 증분 빌드                                      (3~5일)
12. [ ] 조건 섹션 병합 + 런타임 강제 규칙              (2~3일)
13. [ ] C++ Modules (MSVC → Clang)                    (1~2주)
14. [ ] 크로스 컴파일 — iOS/tvOS/watchOS               (3~5일)
15. [ ] 크로스 컴파일 — Android NDK                   (3~5일)
16. [ ] WASM — Emscripten + bare                       (3~5일)
17. [ ] Linux musl + static                            (2~3일)
18. [ ] PCH + Unity Build                              (3~5일)
19. [ ] 패키지 매니저 (PubGrub + git + lock)           (1~2주)
20. [ ] CLI 전체 완성                                  (2~3일)

Phase 2 — 분산 빌드
21. [ ] RemoteExecutor + BuildServer + Worker           (4~6주)

Phase 3 — 생태계
22. [ ] 레지스트리 서버, VS Code, Zed 확장 구현
```

---

## 16. Claude Code 작업 지시 시 권장 사항

- **PLAN.md**: 확정된 타겟/런타임 매트릭스, 파일 확장자 규칙 포함
- **CHECKPOINT.md**: 단계 완료 시 검증된 타겟 목록 기록
- **NativeAOT 제약**: `Reflection`, `dynamic`, `Activator.CreateInstance` 금지
- **구현 순서**: `ProjectLocator` → `ProjectDagBuilder` → 컴파일러 드라이버 순 — 탐색과 DAG가 먼저 있어야 CLI가 동작
- **런타임 강제 규칙**: `RuntimeEnforcer`를 파서 조건 병합 직후 단계로 분리하여 명확히 관리
- **테스트 픽스처**: 단일 프로젝트 / 멀티 프로젝트 DAG / 순환 의존성 케이스 샘플 파일 미리 준비
