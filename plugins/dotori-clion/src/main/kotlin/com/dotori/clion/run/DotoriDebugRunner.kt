package com.dotori.clion.run

import com.dotori.clion.actions.DotoriCli
import com.intellij.execution.ExecutionException
import com.intellij.execution.configurations.GeneralCommandLine
import com.intellij.execution.configurations.RunProfile
import com.intellij.execution.configurations.RunProfileState
import com.intellij.execution.configurations.RunnerSettings
import com.intellij.execution.executors.DefaultDebugExecutor
import com.intellij.execution.process.ProcessHandlerFactory
import com.intellij.execution.process.ProcessTerminatedListener
import com.intellij.execution.runners.ExecutionEnvironment
import com.intellij.execution.runners.GenericProgramRunner
import com.intellij.execution.ui.RunContentDescriptor
import com.intellij.openapi.progress.ProgressManager
import com.intellij.openapi.progress.ProgressIndicator
import com.intellij.openapi.progress.Task
import com.intellij.xdebugger.XDebugProcessStarter
import com.intellij.xdebugger.XDebugSession
import com.intellij.xdebugger.XDebuggerManager
import java.io.File
import java.util.concurrent.atomic.AtomicReference

/**
 * Debug runner for Dotori run configurations.
 *
 * Workflow:
 *  1. Run `dotori build` (debug config) in a modal progress dialog
 *  2. Resolve the output executable path from .dotori-cache/bin/<target>-<config>/<Name>
 *  3. Launch the executable under CLion's XDebugger infrastructure
 */
class DotoriDebugRunner : GenericProgramRunner<RunnerSettings>() {

    override fun getRunnerId(): String = "DotoriDebugRunner"

    override fun canRun(executorId: String, profile: RunProfile): Boolean =
        executorId == DefaultDebugExecutor.EXECUTOR_ID && profile is DotoriRunConfiguration

    override fun doExecute(state: RunProfileState, environment: ExecutionEnvironment): RunContentDescriptor? {
        val config  = environment.runProfile as? DotoriRunConfiguration ?: return null
        val project = environment.project

        val execPathRef = AtomicReference<String?>(null)
        val errorRef    = AtomicReference<String?>(null)

        // ── 1. Build in a modal progress dialog ───────────────────────────
        ProgressManager.getInstance().run(object : Task.Modal(project, "Dotori: Building for debug…", false) {
            override fun run(indicator: ProgressIndicator) {
                indicator.isIndeterminate = true

                val exe = DotoriCli.findExecutable()
                if (exe == null) {
                    errorRef.set("dotori CLI not found. Install dotori and ensure it is in PATH.")
                    return
                }

                val buildArgs = mutableListOf("build")
                if (config.dotoriFilePath.isNotBlank())
                    buildArgs += listOf("--project", config.dotoriFilePath)
                if (config.targetId.isNotBlank())
                    buildArgs += listOf("--target", config.targetId)

                val workDir = resolveWorkDir(config)
                val result  = DotoriCli.run(buildArgs, workDir)
                if (result.exitCode != 0) {
                    errorRef.set("dotori build failed (exit ${result.exitCode}).\n${result.stderr.take(500)}")
                    return
                }

                // ── 2. Resolve executable ──────────────────────────────────
                val projectDir  = workDir ?: return
                val dotoriFile  = resolveDotoriFile(config)
                val projectName = dotoriFile?.let { extractProjectName(it) } ?: File(projectDir).name
                val targetId    = config.targetId.ifBlank { inferHostTarget() }
                val path        = resolveExecutable(projectDir, projectName, targetId, "debug")
                if (path == null) {
                    errorRef.set(
                        "Executable not found after build. Expected:\n" +
                        "$projectDir/.dotori-cache/bin/$targetId-debug/$projectName"
                    )
                    return
                }
                execPathRef.set(path)
            }
        })

        val error = errorRef.get()
        if (error != null) throw ExecutionException(error)

        val exePath = execPathRef.get()
            ?: throw ExecutionException("Build produced no executable.")

        // ── 3. Launch XDebugger ────────────────────────────────────────────
        val runArgs = if (config.runArgs.isNotBlank())
            config.runArgs.split(" ").filter { it.isNotBlank() }
        else
            emptyList()

        val commandLine = GeneralCommandLine(exePath)
            .withParameters(runArgs)
            .withWorkDirectory(resolveWorkDir(config))

        val session = XDebuggerManager.getInstance(project)
            .startSession(environment, object : XDebugProcessStarter() {
                override fun start(session: XDebugSession) = DotoriXDebugProcess(
                    session,
                    ProcessHandlerFactory.getInstance().createColoredProcessHandler(commandLine)
                        .also { ProcessTerminatedListener.attach(it) }
                )
            })
        return session.runContentDescriptor
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private fun resolveWorkDir(config: DotoriRunConfiguration): String? {
        if (config.dotoriFilePath.isNotBlank()) {
            val f = File(config.dotoriFilePath)
            return if (f.isDirectory) f.absolutePath else f.parentFile?.absolutePath
        }
        return config.project?.basePath
    }

    private fun resolveDotoriFile(config: DotoriRunConfiguration): File? {
        if (config.dotoriFilePath.isBlank()) return null
        val f = File(config.dotoriFilePath)
        return when {
            f.isFile      -> f
            f.isDirectory -> File(f, ".dotori").takeIf { it.exists() }
            else          -> null
        }
    }

    private fun extractProjectName(dotoriFile: File): String? {
        val text = dotoriFile.readText()
        return Regex("""\bproject\s+([a-zA-Z_][a-zA-Z0-9_\-]*)""").find(text)?.groupValues?.get(1)
    }

    private fun resolveExecutable(
        projectDir: String,
        projectName: String,
        targetId: String,
        config: String
    ): String? {
        val isWindows = targetId.startsWith("windows") || targetId.startsWith("uwp")
        val isWasm    = targetId.startsWith("wasm32")
        val ext = when {
            isWindows -> ".exe"
            isWasm    -> ".wasm"
            else      -> ""
        }
        val path = "$projectDir/.dotori-cache/bin/$targetId-${config.lowercase()}/$projectName$ext"
        return if (File(path).exists()) path else null
    }

    private fun inferHostTarget(): String {
        val os    = System.getProperty("os.name", "").lowercase()
        val arch  = System.getProperty("os.arch", "").lowercase()
        val isArm = arch.contains("aarch64") || arch.contains("arm")
        return when {
            os.contains("win") -> if (isArm) "windows-arm64" else "windows-x64"
            os.contains("mac") -> if (isArm) "macos-arm64"   else "macos-x64"
            else               -> if (isArm) "linux-arm64"   else "linux-x64"
        }
    }
}
