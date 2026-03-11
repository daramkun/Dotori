package com.dotori.clion.project

import com.dotori.clion.actions.DotoriCli
import com.dotori.clion.lang.DotoriFileType
import com.intellij.ide.impl.OpenProjectTask
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.diagnostic.logger
import com.intellij.openapi.project.Project
import com.intellij.openapi.project.ex.ProjectManagerEx
import com.intellij.openapi.vfs.VfsUtil
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.projectImport.ProjectOpenProcessor
import java.io.File
import java.nio.file.Path
import javax.swing.Icon

/**
 * Allows CLion to open directories containing .dotori files as projects.
 *
 * When a directory (or a .dotori file itself) is opened via File → Open,
 * this processor detects the .dotori file and triggers compile_commands.json generation.
 */
class DotoriProjectOpenProcessor : ProjectOpenProcessor() {

    private val log = logger<DotoriProjectOpenProcessor>()

    override val name: String get() = "Dotori"

    override fun getIcon(file: VirtualFile): Icon = DotoriFileType.ICON

    override fun canOpenProject(file: VirtualFile): Boolean = when {
        file.extension == "dotori" -> true
        file.isDirectory           -> file.children?.any { it.extension == "dotori" } == true
        else                       -> false
    }

    override fun doOpenProject(
        virtualFile: VirtualFile,
        projectToClose: Project?,
        forceOpenInNewFrame: Boolean
    ): Project? {
        val projectDir = if (virtualFile.isDirectory) virtualFile else virtualFile.parent
        val dotoriFile = when {
            virtualFile.extension == "dotori" -> virtualFile
            else -> projectDir.children?.firstOrNull { it.extension == "dotori" }
        } ?: return null

        log.info("Opening dotori project at: ${projectDir.path}")

        val openTask = OpenProjectTask {
            this.projectToClose      = projectToClose
            this.forceOpenInNewFrame = forceOpenInNewFrame
        }
        val project = ProjectManagerEx.getInstanceEx()
            .openProject(Path.of(projectDir.path), openTask) ?: return null

        val settings = DotoriProjectSettings.getInstance(project)
        settings.state.dotoriFilePath = dotoriFile.path

        ApplicationManager.getApplication().executeOnPooledThread {
            generateCompileCommands(project, dotoriFile.path)
        }

        return project
    }

    private fun generateCompileCommands(project: Project, dotoriPath: String) {
        val projectDir = File(dotoriPath).parentFile?.absolutePath ?: return
        val settings   = DotoriProjectSettings.getInstance(project)

        val args = mutableListOf("export", "compile-commands", "--all")
        if (settings.state.targetId.isNotBlank())
            args += listOf("--target", settings.state.targetId)
        if (settings.state.configuration == "release") args += "--release"

        val result = DotoriCli.run(
            args       = args,
            workingDir = projectDir,
            dotoriPath = settings.state.dotoriCliPath.takeIf { it.isNotBlank() },
        )

        if (result.exitCode != 0) {
            log.warn("dotori export compile-commands failed:\n${result.stderr}")
        } else {
            log.info("compile_commands.json generated successfully")
            VfsUtil.findFileByIoFile(File(projectDir, "compile_commands.json"), true)
        }
    }
}
