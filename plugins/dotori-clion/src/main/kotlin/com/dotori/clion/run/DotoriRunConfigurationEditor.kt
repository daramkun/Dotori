package com.dotori.clion.run

import com.intellij.openapi.fileChooser.FileChooserDescriptorFactory
import com.intellij.openapi.options.SettingsEditor
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.TextFieldWithBrowseButton
import com.intellij.ui.components.JBLabel
import com.intellij.ui.components.JBTextField
import com.intellij.util.ui.FormBuilder
import javax.swing.JComboBox
import javax.swing.JPanel

/**
 * Settings UI for the Dotori run configuration.
 */
class DotoriRunConfigurationEditor(private val project: Project) :
    SettingsEditor<DotoriRunConfiguration>() {

    private val dotoriPathField = TextFieldWithBrowseButton().apply {
        addBrowseFolderListener(
            project,
            FileChooserDescriptorFactory.createSingleFileOrFolderDescriptor()
                .withTitle("Select .dotori File or Directory")
                .withDescription("Choose the .dotori project file or a directory containing one")
        )
    }

    private val targetField     = JBTextField()
    private val configCombo     = JComboBox(arrayOf("debug", "release"))
    private val modeCombo       = JComboBox(arrayOf("run", "build"))
    private val runArgsField    = JBTextField()

    private val panel: JPanel = FormBuilder.createFormBuilder()
        .addLabeledComponent(JBLabel("Project (.dotori):"), dotoriPathField, 1, false)
        .addLabeledComponent(JBLabel("Target:"),           targetField,     1, false)
        .addLabeledComponent(JBLabel("Configuration:"),    configCombo,     1, false)
        .addLabeledComponent(JBLabel("Mode:"),             modeCombo,       1, false)
        .addLabeledComponent(JBLabel("Run arguments:"),    runArgsField,    1, false)
        .addComponentFillVertically(JPanel(), 0)
        .panel

    override fun resetEditorFrom(config: DotoriRunConfiguration) {
        dotoriPathField.text          = config.dotoriFilePath
        targetField.text              = config.targetId
        configCombo.selectedItem      = config.configuration
        modeCombo.selectedItem        = config.runMode
        runArgsField.text             = config.runArgs
    }

    override fun applyEditorTo(config: DotoriRunConfiguration) {
        config.dotoriFilePath = dotoriPathField.text.trim()
        config.targetId       = targetField.text.trim()
        config.configuration  = configCombo.selectedItem as? String ?: "debug"
        config.runMode        = modeCombo.selectedItem as? String ?: "run"
        config.runArgs        = runArgsField.text.trim()
    }

    override fun createEditor() = panel
}
