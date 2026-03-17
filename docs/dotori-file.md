# .dotori 파일 구성

`.dotori`는 dotori의 프로젝트 파일입니다. 디렉토리당 하나만 허용되며,
`project` 블록(빌드 대상)과 `package` 블록(배포 명세)을 하나의 파일에 함께 작성할 수 있습니다.

---

## 기본 구조

```
project <이름> {
    # 프로젝트 속성
}

package {
    # 패키지 명세 — 라이브러리 배포 시 작성
}
```

주석은 `# ...` 형식으로 작성합니다.

---

## project 블록

### type — 프로젝트 타입

```
type = executable       # 실행 파일
type = static-library   # 정적 라이브러리
type = shared-library   # 공유 라이브러리 (.dll/.so/.dylib)
type = header-only      # 헤더 전용 라이브러리
```

### std — C++ 표준

```
std = c++17
std = c++20
std = c++23    # 기본값
```

### description — 설명

```
description = "My awesome application"
```

---

## sources 블록 — 소스 파일

글로브 패턴으로 소스 파일을 포함/제외합니다.

```
sources {
    include "src/**/*.cpp"
    include "src/**/*.cc"
    exclude "src/platform/**/*.cpp"    # 특정 경로 제외
}
```

---

## modules 블록 — C++ Modules

```
modules {
    include "src/**/*.cppm"
    include "src/**/*.ixx"
    export-map = true    # module-map.json 자동 생성, 기본: false
}
```

`export-map = true`로 설정하면 `.dotori-cache/obj/<타겟>/bmi/module-map.json`이 생성됩니다.

---

## headers 블록 — 헤더 경로

```
headers {
    public  "include/"    # 의존하는 프로젝트에도 노출
    private "src/"        # 이 프로젝트 내부에서만 사용
}
```

---

## defines 블록 — 전처리기 매크로

```
defines {
    "MY_FEATURE_ENABLED"
    "VERSION_MAJOR=1"
    "VERSION_STRING=\"1.0.0\""
}
```

---

## links 블록 — 링크 라이브러리

```
links {
    "pthread"
    "dl"
    "m"
}
```

---

## frameworks 블록 — Apple 프레임워크

macOS/iOS/tvOS/watchOS 전용입니다.

```
frameworks {
    "Foundation"
    "Metal"
    "MetalKit"
}
```

---

## compile-flags / link-flags — 사용자 정의 플래그

dotori DSL로 제어하기 어려운 컴파일러·링커 옵션을 직접 지정합니다.
이식성을 위해 **조건 블록과 함께 사용**하는 것을 권장합니다.

```
[msvc] {
    compile-flags { "/arch:AVX2" "/fp:fast" }
    link-flags    { "/SUBSYSTEM:WINDOWS" "/OPT:REF" }
}

[clang] {
    compile-flags { "-march=native" "-ffast-math" }
}

[clang.linux] {
    link-flags { "-Wl,--as-needed" "-Wl,--gc-sections" }
}

[release.msvc] {
    compile-flags { "/Oi" "/Ot" }
    link-flags    { "/LTCG" }
}
```

조건 없이 사용하면 `dotori check`에서 이식성 경고가 출력됩니다.

플래그는 dotori가 생성한 플래그 **뒤에** 추가됩니다. 조건 블록 간에는 **누적**됩니다 (덮어쓰기 아님).

---

## runtime-link — 런타임 링크 방식

```
runtime-link = static     # 기본값
runtime-link = dynamic
```

플랫폼별 강제 규칙:

| 플랫폼 | 강제 값 |
|--------|---------|
| UWP | dynamic 강제 |
| iOS / tvOS / watchOS | static 강제 |
| WASM | static 고정 (설정 무시) |

---

## optimize — 최적화 수준

```
optimize = none     # 최적화 없음, Debug 기본
optimize = speed    # 속도 최적화 (-O2 / /O2)
optimize = size     # 크기 최적화 (-Os / /O1)
optimize = full     # 최대 최적화 (-O3 / /Ox /GL)
```

---

## debug-info — 디버그 정보

```
debug-info = full       # 전체 디버그 정보 (-g / /Zi)
debug-info = minimal    # 최소 정보 (-gline-tables-only / /Z7)
debug-info = none       # 디버그 정보 없음
```

---

## lto — 링크 타임 최적화

```
lto = true
lto = false    # 기본값
```

MSVC에서는 `/GL` + `/LTCG`, Clang에서는 `-flto`가 적용됩니다.

---

## warnings — 경고 수준

```
warnings = none
warnings = default    # 기본값
warnings = all        # /W4 / -Wall -Wextra
warnings = extra
```

## warnings-as-errors

```
warnings-as-errors = true
warnings-as-errors = false    # 기본값
```

---

## libc / stdlib — 런타임 라이브러리 선택 (Linux)

```
libc   = glibc      # 기본값
libc   = musl       # musl + static = 완전 정적 바이너리

stdlib = libstdc++  # GCC STL, 기본값
stdlib = libc++     # LLVM STL
```

---

## 플랫폼별 옵션

### Android

```
android-api-level = 26    # 기본: 21
```

### macOS

```
macos-min = "12.0"
```

### iOS

```
ios-min = "15.0"
```

### tvOS / watchOS

```
tvos-min    = "15.0"
watchos-min = "7.0"
```

### Emscripten (WASM)

```
[wasm.emscripten] {
    emscripten-flags { "-sUSE_SDL=2" "-sALLOW_MEMORY_GROWTH" }
}
```

---

## pch 블록 — 프리컴파일 헤더

```
pch {
    header  = "src/pch.h"
    source  = "src/pch.cpp"
    modules = false    # Modules와 PCH 동시 사용 시 경고 출력
}
```

---

## unity-build 블록 — Unity Build

여러 소스 파일을 하나로 묶어 컴파일 속도를 향상시킵니다.

```
unity-build {
    enabled    = true
    batch-size = 8          # 한 번에 묶을 파일 수, 기본: 8
    exclude {
        "src/main.cpp"      # Unity Build에서 제외할 파일
        "src/generated/**"
    }
}
```

C++ Modules 소스 파일(`.cppm`, `.ixx`)은 자동으로 Unity Build에서 제외됩니다.

---

## output 블록 — 출력 디렉토리

빌드 결과물을 지정 경로로 복사합니다. 경로는 `.dotori` 파일 기준 상대 경로입니다.

```
output {
    binaries  = "bin/"     # exe, dll/so/dylib 복사 위치
    libraries = "lib/"     # .lib(import), .a(static) 복사 위치
    symbols   = "pdb/"     # .pdb, .dSYM 복사 위치
}
```

조건 블록과 함께 사용 가능합니다.

```
[release] {
    output { binaries = "dist/" }
}
```

---

## pre-build / post-build 블록 — 빌드 전/후 스크립트

```
pre-build {
    "scripts/gen_version.sh"
    "scripts/download_assets.sh --quiet"
}

post-build {
    "scripts/sign.sh --cert certs/release.p12"
}
```

- 각 문자열은 실행할 명령어입니다.
- 순서대로 실행되며 종료 코드 ≠ 0이면 빌드 실패로 처리됩니다.
- 실행 디렉토리는 프로젝트 루트 (`.dotori` 위치)입니다.
- stdout/stderr가 dotori 콘솔로 연결됩니다.
- `DOTORI_TARGET`, `DOTORI_CONFIG`, `DOTORI_PROJECT_DIR`, `DOTORI_OUTPUT_DIR` 환경 변수가 전달됩니다.

조건 블록과 함께 사용 가능합니다.

```
[windows] {
    pre-build { "scripts\\gen_version.bat" }
}
```

---

## dependencies 블록 — 의존성

```
dependencies {
    # 로컬 경로 의존성 — 빌드 순서 DAG에 포함
    my-lib  = { path = "../lib" }

    # 패키지 레지스트리 버전
    fmt     = "10.2.0"

    # git 의존성
    spdlog  = { git = "https://github.com/gabime/spdlog", tag = "v1.13.0" }
    myutil  = { git = "https://github.com/me/myutil", commit = "abc1234" }

    # 버전 범위 지정
    zlib    = { version = "^1.2.0" }
}
```

`path` 의존성은 빌드 순서 DAG에 포함되어 자동으로 선행 빌드됩니다.
`git`/`version` 의존성은 패키지 매니저가 별도로 처리합니다.

---

## 조건 블록

조건에 따라 다른 설정을 적용합니다.

### 사용 가능한 조건

| 종류 | 값 |
|------|----|
| 플랫폼 | `windows`, `uwp`, `linux`, `android`, `macos`, `ios`, `tvos`, `watchos`, `wasm` |
| 구성 | `debug`, `release` |
| 컴파일러 | `msvc`, `clang` |
| 런타임 링크 | `static`, `dynamic` |
| C 런타임 | `glibc`, `musl` |
| C++ STL | `libcxx`, `libstdcxx` |
| WASM 백엔드 | `emscripten`, `bare` |

### 조건 조합

`.`으로 여러 조건을 조합합니다. 구체적인 조건일수록 우선순위가 높습니다.

```
[windows]               # Windows 전용
[windows.release]       # Windows + Release
[release.msvc]          # Release + MSVC
[clang.linux]           # Clang + Linux
[wasm.emscripten]       # WASM Emscripten
```

```
project MyApp {
    type = executable
    std  = c++23

    [windows] {
        sources { include "src/platform/windows/**/*.cpp" }
        defines { "PLATFORM_WINDOWS" "WIN32_LEAN_AND_MEAN" }
        links   { "kernel32" "user32" }
    }

    [linux] {
        sources { include "src/platform/linux/**/*.cpp" }
        defines { "PLATFORM_LINUX" }
        links   { "pthread" "dl" }
        libc    = glibc
    }

    [macos] {
        sources    { include "src/platform/macos/**/*.cpp" }
        frameworks { "Foundation" "Metal" }
        macos-min  = "12.0"
    }

    [debug] {
        defines    { "DEBUG" "_DEBUG" }
        optimize   = none
        debug-info = full
    }

    [release] {
        defines    { "NDEBUG" }
        optimize   = speed
        lto        = true
    }
}
```

---

## 환경변수 보간 (`${VAR}`)

DSL의 모든 사용자 작성 문자열 안에서 `${VAR}` 형태로 환경변수를 참조할 수 있습니다.
보간은 빌드 시작 시 수행됩니다.

```
headers { public "${BOOST_ROOT}/include/" }
sources { include "${SRC_DIR}/**/*.cpp" }
compile-flags { "-DAPP_VERSION=${APP_VERSION}" }
output  { binaries = "bin/${DOTORI_TARGET}-${DOTORI_CONFIG}/" }
pre-build { "${SCRIPTS_DIR}/gen_version.sh" }
dependencies {
    mylib = { path = "${MYLIB_ROOT}" }
}
```

| 동작 | 설명 |
|------|------|
| `${VAR}` | 환경변수 `VAR` 값으로 치환 |
| 정의되지 않은 변수 | 빈 문자열로 치환 |
| 닫히지 않은 `${` | 리터럴로 유지 |

`type`, `std`, `optimize` 등 DSL 키워드에는 적용되지 않습니다.

dotori가 자동 주입하는 변수:

| 변수 | 예시 |
|------|------|
| `DOTORI_TARGET` | `macos-arm64` |
| `DOTORI_CONFIG` | `debug` |
| `DOTORI_PLATFORM` | `macos` |
| `DOTORI_ARCH` | `arm64` |

---

## package 블록

라이브러리를 레지스트리에 배포할 때 `project` 블록과 함께 작성합니다.

```
package {
    name        = "my-lib"
    version     = "1.0.0"
    description = "My C++ library"
    license     = "MIT"
    homepage    = "https://github.com/me/my-lib"

    authors {
        "Alice <alice@example.com>"
        "Bob <bob@example.com>"
    }

    exports {
        headers = "include/"
    }
}
```

---

## 전체 예시

```
# app/.dotori — 완전한 예시
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
    }

    headers {
        public  "include/"
        private "src/"
    }

    [windows] {
        sources { include "src/platform/windows/**/*.cpp" }
        defines { "PLATFORM_WINDOWS" "WIN32_LEAN_AND_MEAN" }
        links   { "kernel32" "user32" "ole32" }
    }

    [linux] {
        sources { include "src/platform/linux/**/*.cpp" }
        defines { "PLATFORM_LINUX" }
        links   { "pthread" "dl" }
        libc    = glibc
        stdlib  = libstdc++
    }

    [macos] {
        sources    { include "src/platform/macos/**/*.cpp" }
        defines    { "PLATFORM_MACOS" }
        frameworks { "Foundation" "Metal" "MetalKit" }
        macos-min  = "12.0"
    }

    [android] {
        sources           { include "src/platform/android/**/*.cpp" }
        defines           { "PLATFORM_ANDROID" }
        android-api-level = 26
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
        my-lib = { path = "../lib" }
        fmt    = "10.2.0"
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
        binaries  = "bin/"
        libraries = "lib/"
        symbols   = "pdb/"
    }

    [msvc] {
        compile-flags { "/arch:AVX2" }
        link-flags    { "/SUBSYSTEM:WINDOWS" "/OPT:REF" }
    }

    [clang] {
        compile-flags { "-march=native" }
    }

    pre-build  { "scripts/gen_version.sh" }
    post-build { "scripts/sign.sh" }
}
```

---

## 멀티 프로젝트 레이아웃

솔루션 파일 없이 `path` 의존성으로 프로젝트 간 관계를 표현합니다.

```
repo/
├── app/
│   └── .dotori    ← dependencies { my-lib = { path = "../lib" } }
└── lib/
    └── .dotori    ← project MyLib { type = static-library ... }
```

`app/`에서 `dotori build`를 실행하면:

1. `app/.dotori` 로드
2. `path = "../lib"` 의존성 발견 → `lib/.dotori` 로드
3. DAG: `MyLib` → `MyApp` 순서로 자동 빌드

의존 관계가 없는 프로젝트들은 병렬로 빌드됩니다.

---

## Lock 파일 (`.dotori.lock`)

의존성 해결 결과를 저장하는 자동 생성 파일입니다. git에 커밋하여 팀원 간 동일한 의존성 버전을 보장합니다.

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
