package com.dotori.clion.lang

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.psi.PsiElement
import com.intellij.psi.impl.source.tree.LeafPsiElement

/**
 * Annotates `.dotori` files:
 *  - Unknown identifiers that are not in any keyword set → warning
 *  - Highlights known keywords with the correct color (layered on top of the lexer)
 *  - Validates condition blocks: `[unknown-platform]` → warning
 */
class DotoriAnnotator : Annotator {

    // Condition block: platform / config / compiler atoms
    private val CONDITION_ATOMS = setOf(
        "windows", "uwp", "linux", "android", "macos", "ios", "tvos", "watchos", "wasm",
        "debug", "release",
        "msvc", "clang",
        "static", "dynamic", "glibc", "musl", "libcxx", "libstdcxx",
        "emscripten", "bare",
        // arch keywords
        "x64", "x86", "arm64", "arm"
    )

    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is LeafPsiElement) return
        val tokenType = element.elementType

        when (tokenType) {
            DotoriTokenTypes.IDENT -> annotateIdent(element, holder)
        }
    }

    private fun annotateIdent(element: LeafPsiElement, holder: AnnotationHolder) {
        val text = element.text

        // Inside a condition block [...] — validate against known condition atoms
        if (isInsideCondition(element)) {
            if (text !in CONDITION_ATOMS) {
                // Unknown condition atom — warn but allow (custom configs like [myconfig])
                holder.newAnnotation(
                    HighlightSeverity.WEAK_WARNING,
                    "'$text' is not a standard condition keyword"
                ).create()
            }
            return
        }

        val allKnown = DotoriTokenTypes.ALL_KEYWORDS
        if (text !in allKnown) {
            // Unknown identifier at top-level or property position — info-level hint
            holder.newAnnotation(
                HighlightSeverity.WEAK_WARNING,
                "'$text' is not a recognized dotori keyword"
            ).create()
        }
    }

    /**
     * Returns true if [element] is a direct child of a `[...]` condition block,
     * i.e., its parent contains LBRACKET and RBRACKET siblings.
     */
    private fun isInsideCondition(element: PsiElement): Boolean {
        val parent = element.parent ?: return false
        var hasBracket = false
        var child = parent.firstChild
        while (child != null) {
            if (child is LeafPsiElement &&
                (child.elementType == DotoriTokenTypes.LBRACKET ||
                 child.elementType == DotoriTokenTypes.RBRACKET)) {
                hasBracket = true
                break
            }
            child = child.nextSibling
        }
        return hasBracket
    }
}
