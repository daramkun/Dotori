package com.dotori.clion.lang

import com.intellij.lang.Language

object DotoriLanguage : Language("Dotori", "text/x-dotori") {
    private fun readResolve(): Any = DotoriLanguage
    override fun getDisplayName() = "Dotori"
    override fun isCaseSensitive() = true
}
