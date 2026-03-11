package com.dotori.clion.lang

import com.intellij.codeInsight.completion.*
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.patterns.PlatformPatterns
import com.intellij.psi.impl.source.tree.LeafPsiElement
import com.intellij.psi.util.PsiTreeUtil
import com.intellij.util.ProcessingContext

/**
 * Context-aware smart completions for .dotori files.
 *
 * Strategy: scan backwards from the caret to determine what was written on the
 * current assignment line and offer only the values that make sense for that key.
 *
 * Examples:
 *   type =       → executable, static-library, shared-library, header-only
 *   std  =       → c++17, c++20, c++23
 *   optimize =   → none, size, speed, full
 *   debug-info = → none, minimal, full
 *   runtime-link = → static, dynamic
 *   libc =       → glibc, musl
 *   stdlib =     → libc++, libstdc++
 *   lto =        → true, false
 *   warnings =   → none, default, all, extra
 *   warnings-as-errors = → true, false
 *   enabled  =   → true, false   (unity-build)
 *   preLaunchBuild = → (not in dotori — just example)
 *
 * When no specific context is recognized, the generic keyword list is offered
 * (handled by DotoriCompletionContributor already, so we skip here).
 */
class DotoriSmartCompletionContributor : CompletionContributor() {

    init {
        extend(
            CompletionType.BASIC,
            PlatformPatterns.psiElement(DotoriTokenTypes.IDENT).withLanguage(DotoriLanguage),
            DotoriSmartValueProvider
        )
        extend(
            CompletionType.SMART,
            PlatformPatterns.psiElement(DotoriTokenTypes.IDENT).withLanguage(DotoriLanguage),
            DotoriSmartValueProvider
        )
    }

    private object DotoriSmartValueProvider : CompletionProvider<CompletionParameters>() {

        // Map from property keyword → list of allowed values
        private val PROP_VALUES: Map<String, List<String>> = mapOf(
            "type"               to listOf("executable", "static-library", "shared-library", "header-only"),
            "std"                to listOf("c++17", "c++20", "c++23"),
            "optimize"           to listOf("none", "size", "speed", "full"),
            "debug-info"         to listOf("none", "minimal", "full"),
            "runtime-link"       to listOf("static", "dynamic"),
            "libc"               to listOf("glibc", "musl"),
            "stdlib"             to listOf("libc++", "libstdc++"),
            "lto"                to listOf("true", "false"),
            "warnings"           to listOf("none", "default", "all", "extra"),
            "warnings-as-errors" to listOf("true", "false"),
            "enabled"            to listOf("true", "false"),
        )

        override fun addCompletions(
            parameters: CompletionParameters,
            context: ProcessingContext,
            result: CompletionResultSet
        ) {
            val element = parameters.position as? LeafPsiElement ?: return

            // Walk backwards to find key = <caret>  pattern
            val key = findAssignmentKey(element) ?: return

            val values = PROP_VALUES[key] ?: return
            for (v in values) {
                result.addElement(
                    LookupElementBuilder.create(v)
                        .withTypeText(key)
                        .withBoldness(false)
                )
            }
            // Stop other contributors from adding noise when we have specific values
            result.stopHere()
        }

        /**
         * Scans backwards from [element] looking for the pattern:
         *   IDENT("some-key")  EQ  [whitespace*]  <element>
         * Returns the key identifier text if found.
         */
        private fun findAssignmentKey(element: LeafPsiElement): String? {
            // We want: ... key EQ <ws*> element
            var prev: LeafPsiElement? = PsiTreeUtil.prevLeaf(element) as? LeafPsiElement

            // skip whitespace
            while (prev != null && prev.elementType == DotoriTokenTypes.WHITE_SPACE) {
                prev = PsiTreeUtil.prevLeaf(prev) as? LeafPsiElement
            }

            // expect EQ
            if (prev == null || prev.elementType != DotoriTokenTypes.EQ) return null
            prev = PsiTreeUtil.prevLeaf(prev) as? LeafPsiElement

            // skip whitespace
            while (prev != null && prev.elementType == DotoriTokenTypes.WHITE_SPACE) {
                prev = PsiTreeUtil.prevLeaf(prev) as? LeafPsiElement
            }

            // expect IDENT (the key)
            if (prev == null || prev.elementType != DotoriTokenTypes.IDENT) return null
            return prev.text
        }
    }
}
