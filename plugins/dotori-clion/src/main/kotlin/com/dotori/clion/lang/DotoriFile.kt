package com.dotori.clion.lang

import com.intellij.extapi.psi.PsiFileBase
import com.intellij.psi.FileViewProvider

class DotoriFile(viewProvider: FileViewProvider) : PsiFileBase(viewProvider, DotoriLanguage) {
    override fun getFileType() = DotoriFileType.INSTANCE
    override fun toString() = "Dotori File"
}
