package com.dotori.clion.lang

import com.intellij.lexer.LexerBase
import com.intellij.psi.tree.IElementType

/**
 * Hand-written lexer for the .dotori DSL.
 *
 * Token types:
 *  - BLOCK_COMMENT  (* ... *)
 *  - STRING         "..."
 *  - LBRACE / RBRACE / LBRACKET / RBRACKET / EQ
 *  - NUMBER         [0-9]+
 *  - IDENT          [a-zA-Z_][a-zA-Z0-9_\-.+]* (includes keywords)
 *  - WHITE_SPACE    spaces, tabs, newlines
 *  - BAD_CHAR       anything else
 */
class DotoriLexer : LexerBase() {
    private var buffer: CharSequence = ""
    private var startOffset  = 0
    private var endOffset    = 0
    private var bufferEnd    = 0
    private var tokenType: IElementType? = null

    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        this.buffer      = buffer
        this.startOffset = startOffset
        this.endOffset   = startOffset
        this.bufferEnd   = endOffset
        advance()
    }

    override fun getState() = 0

    override fun getTokenType() = tokenType

    override fun getTokenStart() = startOffset

    override fun getTokenEnd() = endOffset

    override fun getBufferSequence() = buffer

    override fun getBufferEnd() = bufferEnd

    override fun advance() {
        startOffset = endOffset
        if (startOffset >= bufferEnd) {
            tokenType = null
            return
        }

        val c = buffer[startOffset]

        // Block comment: (* ... *)
        if (c == '(' && startOffset + 1 < bufferEnd && buffer[startOffset + 1] == '*') {
            endOffset = startOffset + 2
            while (endOffset + 1 < bufferEnd) {
                if (buffer[endOffset] == '*' && buffer[endOffset + 1] == ')') {
                    endOffset += 2
                    break
                }
                endOffset++
            }
            if (endOffset + 1 >= bufferEnd) endOffset = bufferEnd
            tokenType = DotoriTokenTypes.BLOCK_COMMENT
            return
        }

        // String literal
        if (c == '"') {
            endOffset = startOffset + 1
            while (endOffset < bufferEnd && buffer[endOffset] != '"') {
                if (buffer[endOffset] == '\\') endOffset++ // skip escape
                endOffset++
            }
            if (endOffset < bufferEnd) endOffset++ // closing "
            tokenType = DotoriTokenTypes.STRING
            return
        }

        // Whitespace
        if (c.isWhitespace()) {
            endOffset = startOffset + 1
            while (endOffset < bufferEnd && buffer[endOffset].isWhitespace()) endOffset++
            tokenType = DotoriTokenTypes.WHITE_SPACE
            return
        }

        // Single-char tokens
        when (c) {
            '{' -> { endOffset = startOffset + 1; tokenType = DotoriTokenTypes.LBRACE;   return }
            '}' -> { endOffset = startOffset + 1; tokenType = DotoriTokenTypes.RBRACE;   return }
            '[' -> { endOffset = startOffset + 1; tokenType = DotoriTokenTypes.LBRACKET; return }
            ']' -> { endOffset = startOffset + 1; tokenType = DotoriTokenTypes.RBRACKET; return }
            '=' -> { endOffset = startOffset + 1; tokenType = DotoriTokenTypes.EQ;       return }
        }

        // Number
        if (c.isDigit()) {
            endOffset = startOffset + 1
            while (endOffset < bufferEnd && buffer[endOffset].isDigit()) endOffset++
            tokenType = DotoriTokenTypes.NUMBER
            return
        }

        // Identifier or keyword (allow - and . and + for things like c++23, static-library)
        if (c.isLetter() || c == '_') {
            endOffset = startOffset + 1
            while (endOffset < bufferEnd) {
                val nc = buffer[endOffset]
                if (nc.isLetterOrDigit() || nc == '_' || nc == '-' || nc == '.' || nc == '+') endOffset++
                else break
            }
            tokenType = DotoriTokenTypes.IDENT
            return
        }

        // Anything else
        endOffset = startOffset + 1
        tokenType = DotoriTokenTypes.BAD_CHAR
    }
}
