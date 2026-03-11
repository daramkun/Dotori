package com.dotori.clion.project

import com.intellij.openapi.components.*
import com.intellij.openapi.project.Project

/**
 * Persists per-project dotori settings (target, configuration, CLI path override).
 */
@Service(Service.Level.PROJECT)
@State(
    name = "DotoriProjectSettings",
    storages = [Storage(".idea/dotori.xml")]
)
class DotoriProjectSettings : PersistentStateComponent<DotoriProjectSettings.State> {

    data class State(
        var dotoriCliPath: String   = "",    // "" = auto-detect from PATH
        var targetId:      String   = "",    // "" = auto (host OS/arch)
        var configuration: String   = "debug",
        var dotoriFilePath: String  = "",    // path to the root .dotori file
    )

    private var _state = State()

    override fun getState()              = _state
    override fun loadState(state: State) { _state = state }

    companion object {
        fun getInstance(project: Project): DotoriProjectSettings =
            project.service<DotoriProjectSettings>()
    }
}
