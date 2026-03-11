use zed_extension_api::{self as zed, settings::LspSettings, LanguageServerId, Result};
use zed::{BuildTaskDefinition, BuildTaskDefinitionTemplatePayload, DebugScenario, TaskTemplate};

// ── Extension state ───────────────────────────────────────────────────────────

struct DotoriExtension {
    /// Cached path to the clangd binary (found on first use).
    clangd_path: Option<String>,
}

impl DotoriExtension {
    /// Find clangd in common locations.
    /// Priority: user setting → worktree PATH search.
    fn find_clangd(worktree: &zed::Worktree) -> Option<String> {
        // 1. Honour user setting (lsp.clangd.binary.path)
        if let Ok(settings) = LspSettings::for_worktree("clangd", worktree) {
            if let Some(binary) = settings.binary {
                if let Some(path) = binary.path {
                    if !path.is_empty() {
                        return Some(path);
                    }
                }
            }
        }

        // 2. Search PATH via worktree
        let candidates = [
            "clangd",
            "clangd-18",
            "clangd-17",
            "clangd-16",
        ];
        for candidate in &candidates {
            if worktree.which(candidate).is_some() {
                return Some(candidate.to_string());
            }
        }
        None
    }

    /// Infer the host target triple (e.g. "macos-arm64") for executable path resolution.
    fn host_target() -> &'static str {
        #[cfg(all(target_os = "macos",   target_arch = "aarch64"))] return "macos-arm64";
        #[cfg(all(target_os = "macos",   target_arch = "x86_64"))]  return "macos-x64";
        #[cfg(all(target_os = "linux",   target_arch = "aarch64"))] return "linux-arm64";
        #[cfg(all(target_os = "linux",   target_arch = "x86_64"))]  return "linux-x64";
        #[cfg(all(target_os = "windows", target_arch = "x86_64"))]  return "windows-x64";
        #[cfg(all(target_os = "windows", target_arch = "aarch64"))] return "windows-arm64";
        #[allow(unreachable_code)]
        "linux-x64"
    }

    /// Resolve the output executable path.
    /// Convention (from BuildPlanner.cs):
    ///   <project_dir>/.dotori-cache/bin/<target>-<config>/<Name>[.exe|.wasm]
    fn resolve_executable(project_dir: &str, name: &str, target: &str, config: &str) -> String {
        let ext = if target.starts_with("windows") || target.starts_with("uwp") {
            ".exe"
        } else if target.starts_with("wasm32") {
            ".wasm"
        } else {
            ""
        };
        format!("{}/.dotori-cache/bin/{}-{}/{}{}", project_dir, target, config, name, ext)
    }
}

// ── Extension trait implementation ────────────────────────────────────────────

impl zed::Extension for DotoriExtension {
    fn new() -> Self {
        DotoriExtension { clangd_path: None }
    }

    // ── Language server: clangd ────────────────────────────────────────────
    //
    // dotori generates `compile_commands.json` automatically on project open
    // (via `dotori export compile-commands`).  clangd reads that file and
    // provides C++ IntelliSense — Go to Definition, Find References, rename
    // refactoring, diagnostics, and more.

    fn language_server_command(
        &mut self,
        _language_server_id: &LanguageServerId,
        worktree: &zed::Worktree,
    ) -> Result<zed::Command> {
        if self.clangd_path.is_none() {
            self.clangd_path = Self::find_clangd(worktree);
        }
        let path = self.clangd_path.as_deref().unwrap_or("clangd").to_string();

        Ok(zed::Command {
            command: path,
            args: vec![
                "--background-index".to_string(),
                "--clang-tidy".to_string(),
                "--completion-style=detailed".to_string(),
                "--header-insertion=iwyu".to_string(),
                "--log=error".to_string(),
            ],
            env: Default::default(),
        })
    }

    fn language_server_workspace_configuration(
        &mut self,
        _language_server_id: &LanguageServerId,
        _worktree: &zed::Worktree,
    ) -> Result<Option<zed::serde_json::Value>> {
        Ok(Some(zed::serde_json::json!({
            "clangd": {
                "arguments": [
                    "--background-index",
                    "--clang-tidy",
                    "--completion-style=detailed"
                ]
            }
        })))
    }

    // ── Debug locator: dotori ──────────────────────────────────────────────
    //
    // Zed calls this for every task template in the workspace.  We look for
    // dotori build tasks and produce a DebugScenario that points at the
    // compiled executable.

    fn dap_locator_create_scenario(
        &mut self,
        _locator_name: String,
        build_task: TaskTemplate,
        _resolved_label: String,
        _debug_adapter_name: String,
    ) -> Option<DebugScenario> {
        // Only handle tasks whose label contains "dotori" and "build"
        let label = &build_task.label;
        if !label.contains("dotori") || !label.contains("build") {
            return None;
        }

        // Extract working directory
        let cwd = build_task.cwd.as_deref().unwrap_or(".");
        // Strip the Zed variable for path operations
        let worktree_root = cwd.replace("$ZED_WORKTREE_ROOT", ".");

        // Best-effort project name: use cwd directory name
        let project_name = std::path::Path::new(&worktree_root)
            .file_name()
            .and_then(|n| n.to_str())
            .unwrap_or("app")
            .to_string();

        let target     = Self::host_target();
        let executable = Self::resolve_executable(cwd, &project_name, target, "debug");

        // Use lldb on macOS/Linux, cppvsdbg on Windows
        #[cfg(target_os = "windows")]
        let adapter = "cppvsdbg";
        #[cfg(not(target_os = "windows"))]
        let adapter = "lldb";

        // config is a JSON string (per the WIT definition)
        let config_json = zed::serde_json::json!({
            "request":     "launch",
            "program":     executable,
            "cwd":         cwd,
            "args":        [],
            "stopOnEntry": false,
        });

        Some(DebugScenario {
            adapter: adapter.to_string(),
            label: format!("Debug {}", project_name),
            build: Some(BuildTaskDefinition::Template(
                BuildTaskDefinitionTemplatePayload {
                    locator_name: None,
                    template: build_task,
                }
            )),
            config: config_json.to_string(),
            tcp_connection: None,
        })
    }
}

zed::register_extension!(DotoriExtension);
