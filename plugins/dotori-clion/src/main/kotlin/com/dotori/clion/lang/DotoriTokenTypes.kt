package com.dotori.clion.lang

import com.intellij.psi.tree.IElementType
import com.intellij.psi.tree.TokenSet

class DotoriTokenType(debugName: String) : IElementType(debugName, DotoriLanguage) {
    override fun toString() = "DotoriTokenType.${super.toString()}"
}

object DotoriTokenTypes {
    // Structural
    @JvmField val LBRACE      = DotoriTokenType("LBRACE")         // {
    @JvmField val RBRACE      = DotoriTokenType("RBRACE")         // }
    @JvmField val LBRACKET    = DotoriTokenType("LBRACKET")       // [
    @JvmField val RBRACKET    = DotoriTokenType("RBRACKET")       // ]
    @JvmField val EQ          = DotoriTokenType("EQ")             // =

    // Literals
    @JvmField val STRING      = DotoriTokenType("STRING")         // "..."
    @JvmField val IDENT       = DotoriTokenType("IDENT")          // identifier / keyword
    @JvmField val NUMBER      = DotoriTokenType("NUMBER")         // integer literal

    // Comments
    @JvmField val BLOCK_COMMENT = DotoriTokenType("BLOCK_COMMENT") // (*...*)

    // Whitespace
    @JvmField val WHITE_SPACE = DotoriTokenType("WHITE_SPACE")

    // Special
    @JvmField val BAD_CHAR    = DotoriTokenType("BAD_CHAR")

    // Top-level keywords
    val TOP_KEYWORDS = setOf("project", "package")

    // Block keywords inside project
    val BLOCK_KEYWORDS = setOf(
        "sources", "modules", "headers", "defines", "links",
        "frameworks", "dependencies", "pch", "unity-build",
        "authors", "exports"
    )

    // Property keywords
    val PROP_KEYWORDS = setOf(
        "type", "std", "description", "optimize", "debug-info",
        "runtime-link", "libc", "stdlib", "lto", "warnings",
        "warnings-as-errors", "android-api-level", "macos-min",
        "ios-min", "tvos-min", "watchos-min", "emscripten-flags",
        "header", "source", "enabled", "batch-size", "exclude",
        "name", "version", "license", "homepage",
        "include", "exclude", "public", "private",
        "git", "tag", "commit", "path"
    )

    // Value keywords
    val VALUE_KEYWORDS = setOf(
        "executable", "static-library", "shared-library", "header-only",
        "c++17", "c++20", "c++23",
        "none", "size", "speed", "full",
        "minimal",
        "static", "dynamic",
        "glibc", "musl",
        "libc++", "libstdc++",
        "true", "false",
        "windows", "linux", "macos", "ios", "tvos", "watchos", "android", "wasm", "uwp",
        "debug", "release", "msvc", "clang"
    )

    val ALL_KEYWORDS: Set<String> = TOP_KEYWORDS + BLOCK_KEYWORDS + PROP_KEYWORDS + VALUE_KEYWORDS

    // TokenSets for PsiBuilder / highlighting
    @JvmField val KEYWORDS_SET = TokenSet.create(IDENT)
    @JvmField val COMMENT_SET  = TokenSet.create(BLOCK_COMMENT)
    @JvmField val STRING_SET   = TokenSet.create(STRING)
}
