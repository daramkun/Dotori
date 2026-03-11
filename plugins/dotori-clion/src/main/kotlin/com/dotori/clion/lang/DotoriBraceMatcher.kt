package com.dotori.clion.lang

import com.intellij.lang.BracePair
import com.intellij.lang.PairedBraceMatcher
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IElementType

class DotoriBraceMatcher : PairedBraceMatcher {
    private val pairs = arrayOf(
        BracePair(DotoriTokenTypes.LBRACE,   DotoriTokenTypes.RBRACE,   true),
        BracePair(DotoriTokenTypes.LBRACKET, DotoriTokenTypes.RBRACKET, false),
    )

    override fun getPairs() = pairs
    override fun isPairedBracesAllowedBeforeType(lbraceType: IElementType, contextType: IElementType?) = true
    override fun getCodeConstructStart(file: PsiFile, openingBraceOffset: Int) = openingBraceOffset
}
