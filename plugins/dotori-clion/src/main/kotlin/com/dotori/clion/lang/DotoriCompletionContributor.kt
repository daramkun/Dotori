package com.dotori.clion.lang

import com.intellij.codeInsight.completion.*
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext

/**
 * Provides keyword auto-completion for .dotori files.
 * Suggests top-level keywords, block keywords, property names, and values.
 */
class DotoriCompletionContributor : CompletionContributor() {

    init {
        // Complete all known dotori keywords in any position in a dotori file
        extend(
            CompletionType.BASIC,
            PlatformPatterns.psiElement().withLanguage(DotoriLanguage),
            DotoriKeywordCompletionProvider
        )
    }

    private object DotoriKeywordCompletionProvider : CompletionProvider<CompletionParameters>() {
        override fun addCompletions(
            parameters: CompletionParameters,
            context: ProcessingContext,
            result: CompletionResultSet
        ) {
            // Add all keywords as completions
            for (kw in DotoriTokenTypes.TOP_KEYWORDS) {
                result.addElement(LookupElementBuilder.create(kw).bold())
            }
            for (kw in DotoriTokenTypes.BLOCK_KEYWORDS) {
                result.addElement(LookupElementBuilder.create(kw).bold())
            }
            for (kw in DotoriTokenTypes.PROP_KEYWORDS) {
                result.addElement(LookupElementBuilder.create(kw))
            }
            for (kw in DotoriTokenTypes.VALUE_KEYWORDS) {
                result.addElement(
                    LookupElementBuilder.create(kw)
                        .withTypeText("value")
                )
            }
        }
    }
}
