package com.dotori.clion.run

import com.intellij.openapi.fileTypes.FileType
import com.intellij.xdebugger.evaluation.XDebuggerEditorsProvider
import com.dotori.clion.lang.DotoriFileType

/**
 * Minimal editors provider required by XDebugProcess.
 * Since we don't support expression evaluation, this just returns the dotori file type.
 */
class DotoriDebugEditorsProvider : XDebuggerEditorsProvider() {
    override fun getFileType(): FileType = DotoriFileType.INSTANCE
}
