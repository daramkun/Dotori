package com.dotori.clion.lang

import com.intellij.lexer.Lexer
import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.HighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.editor.colors.TextAttributesKey.createTextAttributesKey
import com.intellij.openapi.fileTypes.SyntaxHighlighterBase
import com.intellij.psi.tree.IElementType

class DotoriSyntaxHighlighter : SyntaxHighlighterBase() {

    companion object {
        val COMMENT   = createTextAttributesKey("DOTORI_COMMENT",   DefaultLanguageHighlighterColors.BLOCK_COMMENT)
        val STRING    = createTextAttributesKey("DOTORI_STRING",     DefaultLanguageHighlighterColors.STRING)
        val KEYWORD   = createTextAttributesKey("DOTORI_KEYWORD",    DefaultLanguageHighlighterColors.KEYWORD)
        val NUMBER    = createTextAttributesKey("DOTORI_NUMBER",     DefaultLanguageHighlighterColors.NUMBER)
        val BRACES    = createTextAttributesKey("DOTORI_BRACES",     DefaultLanguageHighlighterColors.BRACES)
        val BRACKETS  = createTextAttributesKey("DOTORI_BRACKETS",   DefaultLanguageHighlighterColors.BRACKETS)
        val OPERATION = createTextAttributesKey("DOTORI_OPERATION",  DefaultLanguageHighlighterColors.OPERATION_SIGN)
        val BAD_CHAR  = createTextAttributesKey("DOTORI_BAD_CHAR",   HighlighterColors.BAD_CHARACTER)

        private val COMMENT_KEYS  = arrayOf(COMMENT)
        private val STRING_KEYS   = arrayOf(STRING)
        private val KEYWORD_KEYS  = arrayOf(KEYWORD)
        private val NUMBER_KEYS   = arrayOf(NUMBER)
        private val BRACES_KEYS   = arrayOf(BRACES)
        private val BRACKETS_KEYS = arrayOf(BRACKETS)
        private val OP_KEYS       = arrayOf(OPERATION)
        private val BAD_KEYS      = arrayOf(BAD_CHAR)
        private val EMPTY_KEYS    = emptyArray<TextAttributesKey>()
    }

    override fun getHighlightingLexer(): Lexer = DotoriLexer()

    override fun getTokenHighlights(tokenType: IElementType): Array<TextAttributesKey> {
        return when (tokenType) {
            DotoriTokenTypes.BLOCK_COMMENT -> COMMENT_KEYS
            DotoriTokenTypes.STRING        -> STRING_KEYS
            DotoriTokenTypes.IDENT         -> KEYWORD_KEYS  // All idents potentially highlighted; real keyword check done in annotator
            DotoriTokenTypes.NUMBER        -> NUMBER_KEYS
            DotoriTokenTypes.LBRACE,
            DotoriTokenTypes.RBRACE        -> BRACES_KEYS
            DotoriTokenTypes.LBRACKET,
            DotoriTokenTypes.RBRACKET      -> BRACKETS_KEYS
            DotoriTokenTypes.EQ            -> OP_KEYS
            DotoriTokenTypes.BAD_CHAR      -> BAD_KEYS
            else                           -> EMPTY_KEYS
        }
    }
}
