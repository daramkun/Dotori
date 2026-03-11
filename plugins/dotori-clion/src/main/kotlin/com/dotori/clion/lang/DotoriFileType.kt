package com.dotori.clion.lang

import com.intellij.openapi.fileTypes.LanguageFileType
import com.intellij.openapi.util.IconLoader
import javax.swing.Icon

class DotoriFileType private constructor() : LanguageFileType(DotoriLanguage) {
    companion object {
        @JvmField
        val INSTANCE = DotoriFileType()

        val ICON: Icon = IconLoader.getIcon("/icons/dotori.svg", DotoriFileType::class.java)
    }

    override fun getName()         = "Dotori"
    override fun getDescription()  = "Dotori build system file"
    override fun getDefaultExtension() = "dotori"
    override fun getIcon()         = ICON
}
