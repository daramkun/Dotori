package com.dotori.clion.run

import com.dotori.clion.actions.DotoriCli
import com.intellij.execution.ExecutionException
import com.intellij.execution.Executor
import com.intellij.execution.configurations.*
import com.intellij.execution.process.ProcessHandlerFactory
import com.intellij.execution.process.ProcessTerminatedListener
import com.intellij.execution.runners.ExecutionEnvironment
import com.intellij.openapi.options.SettingsEditor
import com.intellij.openapi.project.Project
import org.jdom.Element

/**
 * Run configuration for `dotori build` and `dotori run`.
 *
 * The user can configure:
 *  - Which .dotori file to use
 *  - Target (e.g. macos-arm64)
 *  - Configuration (debug/release)
 *  - Whether to just build or also run
 *  - Arguments to pass to the executable (for run mode)
 */
class DotoriRunConfiguration(
    project: Project,
    factory: DotoriRunConfigurationFactory,
    name:    String,
) : RunConfigurationBase<DotoriRunConfigurationOptions>(project, factory, name) {

    // ── Persisted options ──────────────────────────────────────────────────

    var dotoriFilePath: String
        get()    = options.dotoriFilePath
        set(v)   { options.dotoriFilePath = v }

    var targetId: String
        get()    = options.targetId
        set(v)   { options.targetId = v }

    var configuration: String
        get()    = options.configuration
        set(v)   { options.configuration = v }

    var runMode: String           // "build" or "run"
        get()    = options.runMode
        set(v)   { options.runMode = v }

    var runArgs: String           // extra args after "--"
        get()    = options.runArgs
        set(v)   { options.runArgs = v }

    // ── Internals ──────────────────────────────────────────────────────────

    override fun getOptions() = super.getOptions() as DotoriRunConfigurationOptions

    override fun getConfigurationEditor(): SettingsEditor<out RunConfiguration> =
        DotoriRunConfigurationEditor(project)

    override fun getState(executor: Executor, env: ExecutionEnvironment): RunProfileState {
        return object : CommandLineState(env) {
            override fun startProcess(): com.intellij.execution.process.ProcessHandler {
                val exe = DotoriCli.findExecutable()
                    ?: throw ExecutionException(
                        "dotori CLI not found. Please install dotori and ensure it is in PATH."
                    )

                val args = mutableListOf<String>()
                args += runMode   // "build" or "run"

                if (dotoriFilePath.isNotBlank())
                    args += listOf("--project", dotoriFilePath)
                if (targetId.isNotBlank())
                    args += listOf("--target", targetId)
                if (configuration == "release")
                    args += "--release"
                if (runMode == "run" && runArgs.isNotBlank()) {
                    args += "--"
                    args += runArgs.split(" ").filter { it.isNotBlank() }
                }

                val workDir = when {
                    dotoriFilePath.isNotBlank() ->
                        java.io.File(dotoriFilePath).let { if (it.isDirectory) it else it.parentFile }?.absolutePath
                    else -> project.basePath
                }

                val commandLine = com.intellij.execution.configurations.GeneralCommandLine(exe)
                    .withParameters(args)
                    .withWorkDirectory(workDir)

                val processHandler = ProcessHandlerFactory.getInstance()
                    .createColoredProcessHandler(commandLine)
                ProcessTerminatedListener.attach(processHandler)
                return processHandler
            }
        }
    }

    override fun checkConfiguration() {
        if (DotoriCli.findExecutable() == null)
            throw RuntimeConfigurationError(
                "dotori CLI not found. Please install dotori and ensure it is in PATH."
            )
    }
}

/**
 * Persistent options storage for DotoriRunConfiguration.
 */
class DotoriRunConfigurationOptions : RunConfigurationOptions() {
    private var _dotoriFilePath by string("")
    private var _targetId       by string("")
    private var _configuration  by string("debug")
    private var _runMode        by string("run")
    private var _runArgs        by string("")

    var dotoriFilePath: String
        get()    = _dotoriFilePath ?: ""
        set(v)   { _dotoriFilePath = v }

    var targetId: String
        get()    = _targetId ?: ""
        set(v)   { _targetId = v }

    var configuration: String
        get()    = _configuration ?: "debug"
        set(v)   { _configuration = v }

    var runMode: String
        get()    = _runMode ?: "run"
        set(v)   { _runMode = v }

    var runArgs: String
        get()    = _runArgs ?: ""
        set(v)   { _runArgs = v }
}
