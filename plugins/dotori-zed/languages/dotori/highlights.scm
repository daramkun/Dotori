; ── Tree-sitter highlights for .dotori files ─────────────────────────────────
;
; Since dotori does not yet have a published tree-sitter grammar, we define
; highlight queries that will work once one is available.  The grammar should
; expose the node types listed below.  Until then, Zed will fall back to the
; regex-based highlights defined via the language config.

; ── Comments ──────────────────────────────────────────────────────────────────
(block_comment) @comment.block

; ── String literals ───────────────────────────────────────────────────────────
(string) @string

; ── Numbers ───────────────────────────────────────────────────────────────────
(number) @number

; ── Top-level declarations ────────────────────────────────────────────────────
(project_decl "project" @keyword.declaration)
(package_decl "package" @keyword.declaration)

; ── Block keywords ────────────────────────────────────────────────────────────
[
  "sources"
  "modules"
  "headers"
  "defines"
  "links"
  "frameworks"
  "dependencies"
  "pch"
  "unity-build"
  "authors"
  "exports"
] @keyword

; ── Property keywords (left side of `=`) ─────────────────────────────────────
[
  "type"
  "std"
  "description"
  "optimize"
  "debug-info"
  "runtime-link"
  "libc"
  "stdlib"
  "lto"
  "warnings"
  "warnings-as-errors"
  "android-api-level"
  "macos-min"
  "ios-min"
  "tvos-min"
  "watchos-min"
  "emscripten-flags"
  "header"
  "source"
  "enabled"
  "batch-size"
  "exclude"
  "include"
  "public"
  "private"
  "git"
  "tag"
  "commit"
  "path"
  "name"
  "version"
  "license"
  "homepage"
] @property

; ── Value keywords (right side of `=`) ───────────────────────────────────────
[
  "executable"
  "static-library"
  "shared-library"
  "header-only"
  "c++17"
  "c++20"
  "c++23"
  "none"
  "size"
  "speed"
  "full"
  "minimal"
  "static"
  "dynamic"
  "glibc"
  "musl"
  "libc++"
  "libstdc++"
  "true"
  "false"
] @constant.builtin

; ── Condition block identifiers ───────────────────────────────────────────────
(condition_block
  "[" @punctuation.bracket
  (condition_expr) @attribute
  "]" @punctuation.bracket)

; Platform / config atoms inside conditions
[
  "windows"
  "uwp"
  "linux"
  "android"
  "macos"
  "ios"
  "tvos"
  "watchos"
  "wasm"
  "debug"
  "release"
  "msvc"
  "clang"
] @tag

; ── Operators & punctuation ───────────────────────────────────────────────────
"=" @operator

"{" @punctuation.bracket
"}" @punctuation.bracket
"[" @punctuation.bracket
"]" @punctuation.bracket

; ── Project / package name after keyword ─────────────────────────────────────
(project_decl name: (identifier) @type)

; ── Dependency names (left side of dep = {...}) ───────────────────────────────
(dep_item name: (identifier) @variable.member)

; ── General identifiers ───────────────────────────────────────────────────────
(identifier) @variable
