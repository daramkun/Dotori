package com.dotori.clion.run

import com.dotori.clion.lang.DotoriFileType
import com.intellij.execution.actions.ConfigurationContext
import com.intellij.execution.actions.LazyRunConfigurationProducer
import com.intellij.execution.configurations.ConfigurationFactory
import com.intellij.openapi.util.Ref
import com.intellij.psi.PsiElement

/**
 * Automatically creates a dotori run configuration when the user right-clicks
 * on a .dotori file and chooses Run.
 */
class DotoriRunConfigurationProducer :
    LazyRunConfigurationProducer<DotoriRunConfiguration>() {

    override fun getConfigurationFactory(): ConfigurationFactory =
        DotoriRunConfigurationFactory(DotoriRunConfigurationType())

    override fun isConfigurationFromContext(
        configuration: DotoriRunConfiguration,
        context: ConfigurationContext
    ): Boolean {
        val file = context.location?.virtualFile ?: return false
        return file.extension == "dotori" &&
               configuration.dotoriFilePath == file.path
    }

    override fun setupConfigurationFromContext(
        configuration: DotoriRunConfiguration,
        context: ConfigurationContext,
        sourceElement: Ref<PsiElement>
    ): Boolean {
        val file = context.location?.virtualFile ?: return false
        if (file.extension != "dotori" &&
            !file.isDirectory) return false

        val dotoriFile = when {
            file.extension == "dotori" -> file
            file.isDirectory -> file.children?.firstOrNull { it.extension == "dotori" }
            else -> null
        } ?: return false

        configuration.dotoriFilePath = dotoriFile.path
        configuration.name           = "dotori: ${dotoriFile.nameWithoutExtension}"
        return true
    }
}
