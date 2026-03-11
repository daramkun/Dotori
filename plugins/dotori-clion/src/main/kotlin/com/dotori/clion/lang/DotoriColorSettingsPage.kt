package com.dotori.clion.lang

import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.fileTypes.SyntaxHighlighter
import com.intellij.openapi.options.colors.AttributesDescriptor
import com.intellij.openapi.options.colors.ColorDescriptor
import com.intellij.openapi.options.colors.ColorSettingsPage

class DotoriColorSettingsPage : ColorSettingsPage {

    private val descriptors = arrayOf(
        AttributesDescriptor("Comment",   DotoriSyntaxHighlighter.COMMENT),
        AttributesDescriptor("String",    DotoriSyntaxHighlighter.STRING),
        AttributesDescriptor("Keyword",   DotoriSyntaxHighlighter.KEYWORD),
        AttributesDescriptor("Number",    DotoriSyntaxHighlighter.NUMBER),
        AttributesDescriptor("Braces",    DotoriSyntaxHighlighter.BRACES),
        AttributesDescriptor("Brackets",  DotoriSyntaxHighlighter.BRACKETS),
        AttributesDescriptor("Operator",  DotoriSyntaxHighlighter.OPERATION),
        AttributesDescriptor("Bad value", DotoriSyntaxHighlighter.BAD_CHAR),
    )

    override fun getIcon()              = DotoriFileType.ICON
    override fun getHighlighter()       = DotoriSyntaxHighlighter() as SyntaxHighlighter
    override fun getDisplayName()       = "Dotori"
    override fun getAttributeDescriptors() = descriptors
    override fun getColorDescriptors(): Array<ColorDescriptor> = ColorDescriptor.EMPTY_ARRAY
    override fun getAdditionalHighlightingTagToDescriptorMap() = null

    override fun getDemoText() = """
(* This is a dotori project file *)
project MyApp {
    type        = executable
    std         = c++23
    description = "My C++ application"

    sources { include "src/**/*.cpp" }
    headers {
        public  "include/"
        private "src/"
    }

    [debug] {
        defines  { "DEBUG" "_DEBUG" }
        optimize = none
    }

    [release] {
        defines  { "NDEBUG" }
        optimize = speed
        lto      = true
    }

    dependencies {
        my-lib = { path = "../lib" }
    }
}
""".trimIndent()
}
