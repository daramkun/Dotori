package com.dotori.clion.lang

import com.intellij.lang.ASTNode
import com.intellij.lang.ParserDefinition
import com.intellij.lang.PsiParser
import com.intellij.lexer.Lexer
import com.intellij.openapi.project.Project
import com.intellij.psi.FileViewProvider
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IFileElementType
import com.intellij.psi.tree.TokenSet

/**
 * Minimal parser definition for .dotori files.
 * We don't need a full PSI tree for now — just enough for
 * syntax highlighting, brace matching, and completion.
 */
class DotoriParserDefinition : ParserDefinition {

    companion object {
        val FILE = IFileElementType(DotoriLanguage)
    }

    override fun createLexer(project: Project): Lexer = DotoriLexer()

    override fun createParser(project: Project): PsiParser = PsiParser { root, builder ->
        // Flat parse: consume all tokens into the root node
        val marker = builder.mark()
        while (!builder.eof()) builder.advanceLexer()
        marker.done(FILE)
        builder.treeBuilt
    }

    override fun getFileNodeType() = FILE

    override fun getCommentTokens(): TokenSet = DotoriTokenTypes.COMMENT_SET

    override fun getStringLiteralElements(): TokenSet = DotoriTokenTypes.STRING_SET

    override fun getWhitespaceTokens(): TokenSet =
        TokenSet.create(DotoriTokenTypes.WHITE_SPACE)

    override fun createElement(node: ASTNode): PsiElement =
        throw UnsupportedOperationException("No custom PSI elements")

    override fun createFile(viewProvider: FileViewProvider): PsiFile =
        DotoriFile(viewProvider)
}
