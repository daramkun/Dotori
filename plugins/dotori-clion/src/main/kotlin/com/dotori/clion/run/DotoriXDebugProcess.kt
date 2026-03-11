package com.dotori.clion.run

import com.intellij.execution.process.ProcessHandler
import com.intellij.execution.ui.ExecutionConsole
import com.intellij.execution.ui.RunnerLayoutUi
import com.intellij.xdebugger.XDebugProcess
import com.intellij.xdebugger.XDebugSession
import com.intellij.xdebugger.breakpoints.XBreakpointHandler
import com.intellij.xdebugger.evaluation.XDebuggerEditorsProvider
import com.intellij.xdebugger.frame.XSuspendContext

/**
 * Minimal XDebugProcess that wraps the dotori-launched process.
 *
 * This implementation provides a "run under debugger" experience:
 * the process output is shown in the Debug console and the session
 * ends when the process terminates.
 *
 * Full breakpoint/variable inspection support would require integration
 * with CLion's CIDR GDB/LLDB drivers, which is beyond the scope of this
 * MVP integration.
 */
class DotoriXDebugProcess(
    session: XDebugSession,
    private val processHandler: ProcessHandler
) : XDebugProcess(session) {

    init {
        // Notify the session when the process exits
        processHandler.addProcessListener(object : com.intellij.execution.process.ProcessListener {
            override fun processTerminated(event: com.intellij.execution.process.ProcessEvent) {
                session.stop()
            }
        })
    }

    override fun getEditorsProvider(): XDebuggerEditorsProvider =
        DotoriDebugEditorsProvider()

    override fun getBreakpointHandlers(): Array<XBreakpointHandler<*>> =
        emptyArray()  // No breakpoint support in MVP

    override fun createConsole(): ExecutionConsole {
        val console = com.intellij.execution.impl.ConsoleViewImpl(session.project, true)
        console.attachToProcess(processHandler)
        return console
    }

    override fun startStepOver(context: XSuspendContext?) { /* not supported */ }
    override fun startStepInto(context: XSuspendContext?) { /* not supported */ }
    override fun startStepOut(context: XSuspendContext?)  { /* not supported */ }
    override fun resume(context: XSuspendContext?)         { /* not supported */ }
    override fun stop()                                    { processHandler.destroyProcess() }

    override fun doGetProcessHandler(): ProcessHandler = processHandler
}
