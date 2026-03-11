package com.dotori.clion.lang

import com.intellij.lang.ASTNode
import com.intellij.lang.folding.FoldingBuilderEx
import com.intellij.lang.folding.FoldingDescriptor
import com.intellij.openapi.editor.Document
import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.util.TextRange
import com.intellij.psi.PsiElement
import com.intellij.psi.impl.source.tree.LeafPsiElement
import com.intellij.psi.tree.IElementType
import com.intellij.psi.util.PsiTreeUtil

/**
 * Adds code folding for `{ ... }` blocks in .dotori files.
 *
 * Each `{` / `}` pair that spans more than one line becomes a folding region.
 * The placeholder text is `{ ... }` or a smarter label if a keyword precedes the brace.
 */
class DotoriFoldingBuilder : FoldingBuilderEx(), DumbAware {

    override fun buildFoldRegions(
        root: PsiElement,
        document: Document,
        quick: Boolean
    ): Array<FoldingDescriptor> {
        val descriptors = mutableListOf<FoldingDescriptor>()
        collectBraces(root, document, descriptors)
        return descriptors.toTypedArray()
    }

    private fun collectBraces(
        element: PsiElement,
        document: Document,
        descriptors: MutableList<FoldingDescriptor>
    ) {
        if (element is LeafPsiElement && element.elementType == DotoriTokenTypes.LBRACE) {
            val close = findMatchingRbrace(element) ?: return unit()

            val startOffset = element.textRange.startOffset
            val endOffset   = close.textRange.endOffset

            val startLine = document.getLineNumber(startOffset)
            val endLine   = document.getLineNumber(endOffset)

            // Only fold if the block spans more than one line
            if (endLine > startLine) {
                val range = TextRange(startOffset, endOffset)
                descriptors.add(FoldingDescriptor(element.node, range))
            }
        }

        var child = element.firstChild
        while (child != null) {
            collectBraces(child, document, descriptors)
            child = child.nextSibling
        }
    }

    private fun findMatchingRbrace(lbrace: LeafPsiElement): LeafPsiElement? {
        var depth = 0
        var current: PsiElement? = lbrace
        while (current != null) {
            if (current is LeafPsiElement) {
                when (current.elementType) {
                    DotoriTokenTypes.LBRACE -> depth++
                    DotoriTokenTypes.RBRACE -> {
                        depth--
                        if (depth == 0) return current
                    }
                }
            }
            current = PsiTreeUtil.nextLeaf(current)
        }
        return null
    }

    override fun getPlaceholderText(node: ASTNode): String {
        // Find the keyword before the `{` to produce a nicer label
        val psi = node.psi as? LeafPsiElement ?: return "{ ... }"
        val prev = prevMeaningfulLeaf(psi)
        return if (prev != null && prev.elementType == DotoriTokenTypes.IDENT) {
            "{ ... }  // ${prev.text}"
        } else {
            "{ ... }"
        }
    }

    override fun isCollapsedByDefault(node: ASTNode): Boolean = false

    // ── Helpers ────────────────────────────────────────────────────────────

    private fun prevMeaningfulLeaf(element: PsiElement): LeafPsiElement? {
        var current: PsiElement? = PsiTreeUtil.prevLeaf(element)
        while (current != null) {
            if (current is LeafPsiElement &&
                current.elementType != DotoriTokenTypes.WHITE_SPACE &&
                current.elementType != DotoriTokenTypes.BLOCK_COMMENT) {
                return current
            }
            current = PsiTreeUtil.prevLeaf(current)
        }
        return null
    }

    private fun unit(): Unit = Unit
}
