import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { DotoriCli } from './cli';

// ── Helpers ───────────────────────────────────────────────────────────────────

/**
 * Derive the output executable path from a .dotori project.
 *
 * Layout (from BuildPlanner.cs):
 *   <projectDir>/.dotori-cache/bin/<targetId>-<config>/<Name>[.exe|.wasm]
 */
function resolveExecutable(
    projectDir: string,
    projectName: string,
    targetId: string,
    config: string
): string {
    const isWindows = targetId.startsWith('windows') || targetId.startsWith('uwp');
    const isWasm    = targetId.startsWith('wasm32');
    const ext = isWindows ? '.exe' : isWasm ? '.wasm' : '';
    return path.join(
        projectDir, '.dotori-cache', 'bin',
        `${targetId}-${config.toLowerCase()}`,
        `${projectName}${ext}`
    );
}

/**
 * Detect whether ms-vscode.cpptools or vadimcn.vscode-lldb (CodeLLDB) is installed.
 * Returns the debug type string to use in the launch config.
 */
function detectDebuggerType(): 'cppdbg' | 'lldb' | 'cppvsdbg' {
    const extensions = vscode.extensions.all.map(e => e.id.toLowerCase());
    if (extensions.includes('vadimcn.vscode-lldb')) { return 'lldb'; }
    if (extensions.includes('ms-vscode.cpptools'))  {
        // cppvsdbg (MSVC native debugger) on Windows, cppdbg elsewhere
        return process.platform === 'win32' ? 'cppvsdbg' : 'cppdbg';
    }
    // Default: cppdbg (ms-vscode.cpptools)
    return process.platform === 'win32' ? 'cppvsdbg' : 'cppdbg';
}

// ── DotoriDebugConfigurationProvider ─────────────────────────────────────────

export interface DotoriDebugConfiguration extends vscode.DebugConfiguration {
    /** type is always "dotori" */
    type: 'dotori';
    /** Path to .dotori file or directory */
    project?: string;
    /** Build target (e.g. macos-arm64). Empty = use workspace default. */
    target?: string;
    /** "debug" | "release" */
    configuration?: string;
    /** Whether to build before debugging */
    preLaunchBuild?: boolean;
    /** Args to pass to the debuggee */
    args?: string[];
    /** Environment variables for the debuggee */
    env?: Record<string, string>;
    /** Stop at entry (main) */
    stopAtEntry?: boolean;
}

export class DotoriDebugConfigurationProvider
    implements vscode.DebugConfigurationProvider {

    // ── provideDebugConfigurations ────────────────────────────────────────
    // Called when the user opens the "create launch.json" flow.

    async provideDebugConfigurations(
        folder: vscode.WorkspaceFolder | undefined
    ): Promise<DotoriDebugConfiguration[]> {
        if (!folder) { return [this.defaultConfig()]; }

        // Find .dotori files and generate one config per project
        const files = await vscode.workspace.findFiles(
            new vscode.RelativePattern(folder, '**/.dotori'),
            '**/.dotori-cache/**',
            20
        );

        if (files.length === 0) { return [this.defaultConfig()]; }

        return files.map(uri => {
            const raw  = fs.readFileSync(uri.fsPath, 'utf8');
            const name = DotoriCli.extractProjectName(raw)
                ?? path.basename(path.dirname(uri.fsPath));
            return {
                type:          'dotori',
                request:       'launch',
                name:          `Debug ${name}`,
                project:       uri.fsPath,
                target:        '',
                configuration: 'debug',
                preLaunchBuild: true,
                stopAtEntry:   false,
                args:          [],
                env:           {},
            };
        });
    }

    // ── resolveDebugConfiguration ─────────────────────────────────────────
    // Called just before a debug session starts — we translate the dotori
    // config into a concrete cppdbg / cppvsdbg / lldb launch config.

    async resolveDebugConfiguration(
        folder: vscode.WorkspaceFolder | undefined,
        config: vscode.DebugConfiguration
    ): Promise<vscode.DebugConfiguration | undefined> {
        const dc = config as DotoriDebugConfiguration;

        // ── 1. Resolve project directory & name ───────────────────────────
        let dotoriPath = dc.project?.trim();
        const wsRoot   = folder?.uri.fsPath ?? vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;

        if (!dotoriPath && wsRoot) {
            // Find first .dotori in workspace
            const found = await vscode.workspace.findFiles('**/.dotori', '**/.dotori-cache/**', 1);
            dotoriPath  = found[0]?.fsPath;
        }

        if (!dotoriPath) {
            void vscode.window.showErrorMessage('Dotori: cannot find .dotori project file.');
            return undefined;
        }

        // Normalize: if user pointed to a directory, append .dotori
        const stat = fs.statSync(dotoriPath, { throwIfNoEntry: false });
        if (stat?.isDirectory()) {
            dotoriPath = path.join(dotoriPath, '.dotori');
        }

        if (!fs.existsSync(dotoriPath)) {
            void vscode.window.showErrorMessage(`Dotori: .dotori file not found at ${dotoriPath}`);
            return undefined;
        }

        const projectDir  = path.dirname(dotoriPath);
        const raw         = fs.readFileSync(dotoriPath, 'utf8');
        const projectName = DotoriCli.extractProjectName(raw) ?? path.basename(projectDir);

        // ── 2. Resolve target + config ────────────────────────────────────
        const wsCfg    = vscode.workspace.getConfiguration('dotori');
        const targetId = (dc.target?.trim() || wsCfg.get<string>('defaultTarget', '').trim() || inferHostTarget());
        const buildCfg = dc.configuration?.trim() || wsCfg.get<string>('defaultConfiguration', 'debug');

        // ── 3. Build before launching if requested ────────────────────────
        if (dc.preLaunchBuild !== false) {
            const built = await this.runBuild(dotoriPath, targetId, buildCfg);
            if (!built) { return undefined; }
        }

        // ── 4. Resolve executable path ────────────────────────────────────
        const exePath = resolveExecutable(projectDir, projectName, targetId, buildCfg);
        if (!fs.existsSync(exePath)) {
            void vscode.window.showErrorMessage(
                `Dotori: executable not found at ${exePath}. Build may have failed.`
            );
            return undefined;
        }

        // ── 5. Build the concrete launch config ───────────────────────────
        const debuggerType = detectDebuggerType();
        const miDebuggerPath = debuggerType === 'cppdbg'
            ? await findGdbOrLldbPath()
            : undefined;

        return this.buildLaunchConfig(
            debuggerType, exePath, projectDir,
            dc.args ?? [], dc.env ?? {},
            dc.stopAtEntry ?? false,
            miDebuggerPath
        );
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private defaultConfig(): DotoriDebugConfiguration {
        return {
            type:          'dotori',
            request:       'launch',
            name:          'Debug dotori project',
            project:       '${workspaceFolder}/.dotori',
            target:        '',
            configuration: 'debug',
            preLaunchBuild: true,
            stopAtEntry:   false,
            args:          [],
            env:           {},
        };
    }

    private async runBuild(dotoriPath: string, targetId: string, config: string): Promise<boolean> {
        return await vscode.window.withProgress(
            {
                location: vscode.ProgressLocation.Notification,
                title: 'Dotori: Building before debug…',
                cancellable: false,
            },
            async () => {
                const args = ['build', '--project', dotoriPath];
                if (config === 'release') { args.push('--release'); }
                if (targetId) { args.push('--target', targetId); }

                const result = await DotoriCli.run(args, path.dirname(dotoriPath), 300_000);
                if (result.exitCode !== 0) {
                    void vscode.window.showErrorMessage(
                        `Dotori: build failed (exit ${result.exitCode}). Check the terminal output.`
                    );
                    return false;
                }
                return true;
            }
        );
    }

    private buildLaunchConfig(
        type: 'cppdbg' | 'lldb' | 'cppvsdbg',
        program: string,
        cwd: string,
        args: string[],
        env: Record<string, string>,
        stopAtEntry: boolean,
        miDebuggerPath?: string
    ): vscode.DebugConfiguration {
        if (type === 'lldb') {
            // CodeLLDB format
            return {
                type:        'lldb',
                request:     'launch',
                name:        'Dotori (lldb)',
                program,
                args,
                cwd,
                env,
                stopOnEntry: stopAtEntry,
            };
        }

        if (type === 'cppvsdbg') {
            // ms-vscode.cpptools Windows native debugger
            return {
                type:        'cppvsdbg',
                request:     'launch',
                name:        'Dotori (cppvsdbg)',
                program,
                args,
                cwd,
                environment: Object.entries(env).map(([name, value]) => ({ name, value })),
                stopAtEntry,
            };
        }

        // cppdbg (GDB / LLDB via mi)
        const cfg: vscode.DebugConfiguration = {
            type:        'cppdbg',
            request:     'launch',
            name:        'Dotori (cppdbg)',
            program,
            args,
            cwd,
            environment: Object.entries(env).map(([name, value]) => ({ name, value })),
            stopAtEntry,
            externalConsole: false,
            MIMode: process.platform === 'darwin' ? 'lldb' : 'gdb',
        };
        if (miDebuggerPath) { cfg['miDebuggerPath'] = miDebuggerPath; }
        return cfg;
    }
}

// ── Host target inference ─────────────────────────────────────────────────────

function inferHostTarget(): string {
    const p = process.platform;
    const a = process.arch;
    if (p === 'win32')  { return a === 'arm64' ? 'windows-arm64' : a === 'ia32' ? 'windows-x86' : 'windows-x64'; }
    if (p === 'darwin') { return a === 'arm64' ? 'macos-arm64' : 'macos-x64'; }
    return a === 'arm64' ? 'linux-arm64' : 'linux-x64';
}

// ── Debugger path lookup ──────────────────────────────────────────────────────

async function findGdbOrLldbPath(): Promise<string | undefined> {
    const candidates = process.platform === 'darwin'
        ? ['lldb', 'lldb-17', 'lldb-16']
        : ['gdb', 'lldb', 'lldb-17', 'gdb-14', 'gdb-13'];

    const pathEnv = process.env['PATH'] ?? '';
    for (const dir of pathEnv.split(path.delimiter)) {
        for (const name of candidates) {
            const full = path.join(dir, name);
            if (fs.existsSync(full)) { return full; }
        }
    }
    return undefined;
}
