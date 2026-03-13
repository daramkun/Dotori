import * as vscode from 'vscode';

/** Documentation for every keyword/value recognised by the dotori DSL. */
const KEYWORD_DOCS: Record<string, string> = {
    // ── Top-level blocks ────────────────────────────────────────────────────
    'project':      'Declares a build project. Usage: `project <Name> { ... }`',
    'package':      'Declares a package for distribution. Usage: `package { name = "..." version = "..." ... }`',

    // ── Project properties ──────────────────────────────────────────────────
    'type':         'Project output type.\n\nValues: `executable` | `static-library` | `shared-library` | `header-only`',
    'std':          'C++ standard to use.\n\nValues: `c++17` | `c++20` | `c++23`',
    'description':  'Human-readable description of the project.',
    'optimize':     'Optimization level.\n\nValues: `none` | `speed` | `size` | `full`',
    'debug-info':   'Debug information level.\n\nValues: `none` | `minimal` | `full`',
    'runtime-link': 'Link mode for the C/C++ runtime.\n\nValues: `static` | `dynamic`\n\n*Note*: iOS/tvOS/watchOS force `static`; UWP forces `dynamic`.',
    'libc':         'C runtime to use (Linux only).\n\nValues: `glibc` | `musl`',
    'stdlib':       'C++ standard library.\n\nValues: `libc++` | `libstdc++`',
    'lto':          'Enable Link Time Optimization.\n\nValues: `true` | `false`',
    'warnings':     'Warning level.\n\nValues: `none` | `default` | `all` | `extra`',
    'warnings-as-errors': 'Treat all warnings as errors.\n\nValues: `true` | `false`',
    'android-api-level':  'Minimum Android API level (e.g. `26`).',
    'macos-min':    'Minimum macOS version (e.g. `"12.0"`).',
    'ios-min':      'Minimum iOS version (e.g. `"15.0"`).',
    'tvos-min':     'Minimum tvOS version.',
    'watchos-min':  'Minimum watchOS version.',
    'manifest':     'Path to a Windows `.manifest` file to embed after linking.',

    // ── Blocks ──────────────────────────────────────────────────────────────
    'sources':       'Source file glob patterns.\n\nUsage:\n```\nsources {\n    include "src/**/*.cpp"\n    exclude "src/generated/**"\n}\n```',
    'modules':       'C++20 module source files (`.cppm`, `.ixx`).\n\nGenerates BMI files at build time.',
    'headers':       'Header search paths, classified as `public` or `private`.\n\nPublic headers are exposed to dependents.',
    'defines':       'Preprocessor definitions.\n\nUsage: `defines { "NDEBUG" "MY_MACRO=1" }`',
    'links':         'Libraries to link against.\n\nUsage: `links { "pthread" "dl" }`',
    'frameworks':    'Apple frameworks to link (macOS/iOS/tvOS/watchOS).\n\nUsage: `frameworks { "Foundation" "Metal" }`',
    'framework-paths': 'Paths to `.framework` or `.xcframework` bundles. Resolved at build time.',
    'resources':     'Windows resource files (`.rc`) to compile with `rc.exe`.',
    'compile-flags': 'Raw compiler flags appended after dotori-generated flags.',
    'link-flags':    'Raw linker flags appended after dotori-generated flags.',
    'emscripten-flags': 'Extra flags passed to `emcc` when targeting WebAssembly via Emscripten.',
    'dependencies':  'Project or package dependencies.\n\nExamples:\n```\ndependencies {\n    my-lib = { path = "../lib" }\n    fmt    = "10.2.0"\n    spdlog = { git = "https://github.com/gabime/spdlog", tag = "v1.13.0" }\n}\n```',
    'pch':           'Precompiled header configuration.\n\nUsage:\n```\npch {\n    header = "src/pch.h"\n    source = "src/pch.cpp"\n}\n```',
    'unity-build':   'Unity (jumbo) build settings.\n\nBatches multiple source files into a single compilation unit.\n\nUsage:\n```\nunity-build {\n    enabled    = true\n    batch-size = 8\n    exclude    { "src/main.cpp" }\n}\n```',
    'output':        'Directories for build artifacts (relative to project root).\n\nUsage:\n```\noutput {\n    binaries  = "bin/"    (* exe, dll/so/dylib *)\n    libraries = "lib/"    (* .a, import .lib  *)\n    symbols   = "pdb/"    (* .pdb, .dSYM       *)\n}\n```',
    'pre-build':     'Shell commands to run **before** compilation.\n\nEnvironment: `DOTORI_TARGET`, `DOTORI_CONFIG`, `DOTORI_PROJECT_DIR`, `DOTORI_OUTPUT_DIR`',
    'post-build':    'Shell commands to run **after** linking and artifact copy.\n\nEnvironment: `DOTORI_TARGET`, `DOTORI_CONFIG`, `DOTORI_PROJECT_DIR`, `DOTORI_OUTPUT_DIR`',

    // ── Dependency options ──────────────────────────────────────────────────
    'path':    'Local path dependency (included in the build DAG).',
    'git':     'Git repository URL for a dependency.',
    'tag':     'Git tag to check out for a git dependency.',
    'commit':  'Git commit hash for a git dependency.',
    'version': 'Version constraint for a dependency.',

    // ── Project types ───────────────────────────────────────────────────────
    'executable':      'Produces an executable binary.',
    'static-library':  'Produces a static library (`.a` / `.lib`).',
    'shared-library':  'Produces a shared library (`.so` / `.dll` / `.dylib`).',
    'header-only':     'Header-only library; no compilation step.',

    // ── Condition atoms ─────────────────────────────────────────────────────
    'windows':   'Condition: target is Windows (desktop).',
    'uwp':       'Condition: target is Universal Windows Platform.',
    'linux':     'Condition: target is Linux.',
    'android':   'Condition: target is Android (NDK).',
    'macos':     'Condition: target is macOS.',
    'ios':       'Condition: target is iOS.',
    'tvos':      'Condition: target is tvOS.',
    'watchos':   'Condition: target is watchOS.',
    'wasm':      'Condition: target is WebAssembly.',
    'debug':     'Condition: build configuration is Debug.',
    'release':   'Condition: build configuration is Release.',
    'msvc':      'Condition: compiler is MSVC.',
    'clang':     'Condition: compiler is Clang.',
    'static':    'Condition: runtime-link is static.',
    'dynamic':   'Condition: runtime-link is dynamic.',
    'glibc':     'Condition: libc is glibc.',
    'musl':      'Condition: libc is musl.',
    'libcxx':    'Condition: stdlib is libc++.',
    'libstdcxx': 'Condition: stdlib is libstdc++.',
    'emscripten':'Condition: WASM backend is Emscripten.',
    'bare':      'Condition: WASM backend is bare clang (wasm32-unknown-unknown).',

    // ── PCH fields ──────────────────────────────────────────────────────────
    'header':      'Precompiled header file path (`.h`).',
    'source':      'Precompiled header source file path (`.cpp`) — MSVC only.',

    // ── Unity-build fields ──────────────────────────────────────────────────
    'enabled':     'Enable unity build.\n\nValues: `true` | `false`',
    'batch-size':  'Number of source files per unity batch (default: `8`).',
    'exclude':     'Glob patterns for files to exclude from unity batching.',

    // ── Output fields ───────────────────────────────────────────────────────
    'binaries':   'Output directory for executables and shared libraries.',
    'libraries':  'Output directory for static libraries and Windows import libs.',
    'symbols':    'Output directory for debug symbols (`.pdb`, `.dSYM`).',

    // ── Package fields ──────────────────────────────────────────────────────
    'name':        'Package name.',
    'license':     'SPDX license identifier (e.g. `"MIT"`, `"Apache-2.0"`).',
    'homepage':    'Package homepage URL.',
    'authors':     'List of package authors.',
    'exports':     'Module or header path exports for consumers.',
    'export-map':  'Generate a module export map (`.json`) alongside BMI files.\n\nValues: `true` | `false` (default: `true`)',
};

export class DotoriHoverProvider implements vscode.HoverProvider {

    provideHover(
        document: vscode.TextDocument,
        position: vscode.Position,
    ): vscode.ProviderResult<vscode.Hover> {

        const wordRange = document.getWordRangeAtPosition(position, /[a-zA-Z][a-zA-Z0-9_\-]*/);
        if (!wordRange) { return undefined; }

        const word = document.getText(wordRange);
        const doc  = KEYWORD_DOCS[word];
        if (!doc) { return undefined; }

        const md = new vscode.MarkdownString();
        md.isTrusted = true;
        md.appendMarkdown(`**\`${word}\`** — ${doc}`);

        return new vscode.Hover(md, wordRange);
    }
}
