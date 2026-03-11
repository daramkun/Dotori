package com.dotori.clion.lang

import com.intellij.lang.CodeDocumentationAwareCommenter
import com.intellij.psi.PsiComment
import com.intellij.psi.tree.IElementType

class DotoriCommenter : CodeDocumentationAwareCommenter {
    override fun getLineCommentPrefix()             = null    // no line comments in dotori DSL
    override fun getBlockCommentPrefix()            = "(*"
    override fun getBlockCommentSuffix()            = "*)"
    override fun getCommentedBlockCommentPrefix()   = null
    override fun getCommentedBlockCommentSuffix()   = null
    override fun getDocumentationCommentPrefix()    = null
    override fun getDocumentationCommentLinePrefix() = null
    override fun getDocumentationCommentSuffix()    = null
    override fun isDocumentationComment(element: PsiComment) = false
    override fun getDocumentationCommentTokenType(): IElementType? = null
    override fun getLineCommentTokenType(): IElementType?  = null
    override fun getBlockCommentTokenType(): IElementType  = DotoriTokenTypes.BLOCK_COMMENT
}
