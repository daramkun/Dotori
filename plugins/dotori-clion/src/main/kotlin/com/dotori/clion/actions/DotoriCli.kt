package com.dotori.clion.actions

import com.intellij.openapi.diagnostic.logger
import java.io.File
import java.util.concurrent.TimeUnit

/**
 * Utility for invoking the dotori CLI.
 * Searches for the dotori executable in PATH.
 */
object DotoriCli {

    private val log = logger<DotoriCli>()

    data class Result(
        val exitCode: Int,
        val stdout:   String,
        val stderr:   String,
    )

    /**
     * Find the dotori executable in PATH.
     * Returns null if not found.
     */
    fun findExecutable(): String? {
        val isWindows = System.getProperty("os.name", "").lowercase().startsWith("win")
        val candidates = if (isWindows) listOf("dotori.exe", "dotori.cmd", "dotori") else listOf("dotori")

        val pathDirs = (System.getenv("PATH") ?: "").split(File.pathSeparator)
        for (dir in pathDirs) {
            for (name in candidates) {
                val f = File(dir, name)
                if (f.isFile && f.canExecute()) return f.absolutePath
            }
        }
        return null
    }

    /**
     * Run dotori with the given arguments.
     * @param args Arguments to pass (not including the executable name)
     * @param workingDir Working directory for the process
     * @param timeoutSeconds Timeout in seconds (default 60)
     */
    fun run(
        args:           List<String>,
        workingDir:     String? = null,
        timeoutSeconds: Long    = 60L,
        dotoriPath:     String? = null,
    ): Result {
        val exe = dotoriPath?.takeIf { it.isNotBlank() } ?: findExecutable()
        if (exe == null) {
            log.warn("dotori executable not found in PATH")
            return Result(-1, "", "dotori not found in PATH")
        }

        val command = listOf(exe) + args
        log.debug("Running: ${command.joinToString(" ")}")

        return try {
            val pb = ProcessBuilder(command).apply {
                if (workingDir != null) directory(File(workingDir))
                redirectErrorStream(false)
            }
            val proc = pb.start()

            val stdoutFuture = proc.inputStream.bufferedReader().readText()
            val stderrFuture = proc.errorStream.bufferedReader().readText()

            val exited = proc.waitFor(timeoutSeconds, TimeUnit.SECONDS)
            if (!exited) {
                proc.destroyForcibly()
                return Result(-1, stdoutFuture, "Process timed out after ${timeoutSeconds}s")
            }

            Result(proc.exitValue(), stdoutFuture, stderrFuture)
        } catch (e: Exception) {
            log.warn("Failed to run dotori: ${e.message}", e)
            Result(-1, "", e.message ?: "Unknown error")
        }
    }
}
