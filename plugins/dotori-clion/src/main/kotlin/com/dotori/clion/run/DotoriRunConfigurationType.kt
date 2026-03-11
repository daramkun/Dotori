package com.dotori.clion.run

import com.dotori.clion.lang.DotoriFileType
import com.intellij.execution.configurations.ConfigurationTypeBase
import com.intellij.execution.configurations.RunConfiguration
import com.intellij.openapi.project.Project

class DotoriRunConfigurationType : ConfigurationTypeBase(
    "DotoriRunConfiguration",
    "dotori",
    "Run or build a dotori C++ project",
    DotoriFileType.ICON
) {
    companion object {
        const val ID = "DotoriRunConfiguration"
    }

    init {
        addFactory(DotoriRunConfigurationFactory(this))
    }
}

class DotoriRunConfigurationFactory(type: DotoriRunConfigurationType) :
    com.intellij.execution.configurations.ConfigurationFactory(type) {

    override fun getId() = DotoriRunConfigurationType.ID

    override fun createTemplateConfiguration(project: Project): RunConfiguration =
        DotoriRunConfiguration(project, this, "dotori Run")
}
