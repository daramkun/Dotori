package com.dotori.clion.actions

import com.dotori.clion.project.DotoriProjectSettings
import com.intellij.notification.NotificationGroupManager
import com.intellij.notification.NotificationType
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.diagnostic.logger
import com.intellij.openapi.progress.ProgressIndicator
import com.intellij.openapi.progress.ProgressManager
import com.intellij.openapi.progress.Task
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VfsUtil
import java.io.File

/**
 * Action: Tools → Refresh dotori compile_commands.json
 * Runs `dotori export compile-commands` and notifies the user.
 */
class DotoriRefreshAction : AnAction() {

    private val log = logger<DotoriRefreshAction>()

    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return

        ProgressManager.getInstance().run(object : Task.Backgroundable(
            project,
            "Refreshing dotori compile_commands.json",
            true
        ) {
            override fun run(indicator: ProgressIndicator) {
                indicator.text = "Running dotori export compile-commands..."
                indicator.isIndeterminate = true

                val settings   = DotoriProjectSettings.getInstance(project)
                val projectDir = when {
                    settings.state.dotoriFilePath.isNotBlank() ->
                        File(settings.state.dotoriFilePath).parent
                    else -> project.basePath
                } ?: return

                val args = mutableListOf("export", "compile-commands", "--all")
                if (settings.state.targetId.isNotBlank())
                    args += listOf("--target", settings.state.targetId)
                if (settings.state.configuration == "release") args += "--release"

                val result = DotoriCli.run(
                    args        = args,
                    workingDir  = projectDir,
                    dotoriPath  = settings.state.dotoriCliPath.takeIf { it.isNotBlank() },
                )

                ApplicationManager.getApplication().invokeLater {
                    // Refresh VFS
                    VfsUtil.findFileByIoFile(File(projectDir, "compile_commands.json"), true)

                    val group = NotificationGroupManager.getInstance()
                        .getNotificationGroup("Dotori")

                    if (result.exitCode == 0) {
                        group?.createNotification(
                            "compile_commands.json refreshed",
                            NotificationType.INFORMATION
                        )?.notify(project)
                    } else {
                        group?.createNotification(
                            "dotori export failed",
                            result.stderr.ifBlank { "Unknown error" },
                            NotificationType.ERROR
                        )?.notify(project)
                    }
                }
            }
        })
    }

    override fun update(e: AnActionEvent) {
        // Only show if this is a dotori project
        val project = e.project
        e.presentation.isEnabled = project != null
    }
}
