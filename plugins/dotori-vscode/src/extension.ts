import * as vscode from 'vscode';
import { DotoriCli } from './cli';
import { DotoriTaskProvider } from './taskProvider';
import { DotoriStatusBar } from './statusBar';
import { DotoriDebugConfigurationProvider } from './debugConfigProvider';

// ── Known build targets for quick-pick ───────────────────────────────────────
const KNOWN_TARGETS = [
    { label: '$(chip) auto',                description: 'Auto-detect from host OS/arch' },
    { label: '$(chip) windows-x64',          description: 'Windows AMD64' },
    { label: '$(chip) windows-x86',          description: 'Windows 32-bit' },
    { label: '$(chip) windows-arm64',        description: 'Windows ARM64' },
    { label: '$(chip) linux-x64',            description: 'Linux AMD64' },
    { label: '$(chip) linux-arm64',          description: 'Linux ARM64' },
    { label: '$(chip) macos-arm64',          description: 'macOS Apple Silicon' },
    { label: '$(chip) macos-x64',            description: 'macOS Intel' },
    { label: '$(chip) ios-arm64',            description: 'iOS ARM64 (cross)' },
    { label: '$(chip) android-arm64',        description: 'Android ARM64 (NDK)' },
    { label: '$(chip) android-x64',          description: 'Android AMD64 (NDK)' },
    { label: '$(chip) wasm32-emscripten',    description: 'WebAssembly (Emscripten)' },
    { label: '$(chip) wasm32-bare',          description: 'WebAssembly (bare clang)' },
];

let statusBar: DotoriStatusBar;

export function activate(context: vscode.ExtensionContext): void {

    // ── Status bar ────────────────────────────────────────────────────────
    statusBar = new DotoriStatusBar(context);
    statusBar.update();

    // ── Task provider ─────────────────────────────────────────────────────
    context.subscriptions.push(
        vscode.tasks.registerTaskProvider(
            DotoriTaskProvider.taskType,
            new DotoriTaskProvider()
        )
    );

    // ── Commands ──────────────────────────────────────────────────────────
    context.subscriptions.push(
        vscode.commands.registerCommand('dotori.build',       () => runTask('build',  false)),
        vscode.commands.registerCommand('dotori.buildRelease',() => runTask('build',  true)),
        vscode.commands.registerCommand('dotori.run',         () => runTask('run',    false)),
        vscode.commands.registerCommand('dotori.clean',       () => runTask('clean',  false)),
        vscode.commands.registerCommand('dotori.exportCompileCommands', exportCompileCommands),
        vscode.commands.registerCommand('dotori.selectTarget', selectTarget),
        vscode.commands.registerCommand('dotori.selectConfig', selectConfig),
    );

    // ── Debug configuration provider ─────────────────────────────────────
    const debugProvider = new DotoriDebugConfigurationProvider();
    context.subscriptions.push(
        vscode.debug.registerDebugConfigurationProvider('dotori', debugProvider)
    );

    // ── React to config changes ───────────────────────────────────────────
    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration(e => {
            if (e.affectsConfiguration('dotori')) {
                statusBar.update();
            }
        })
    );

    // ── Set context key (used in `when` clauses) ──────────────────────────
    updateWorkspaceContext();
    context.subscriptions.push(
        vscode.workspace.onDidChangeWorkspaceFolders(() => updateWorkspaceContext())
    );

    // ── Auto-export compile_commands.json on activation ───────────────────
    const cfg = vscode.workspace.getConfiguration('dotori');
    if (cfg.get<boolean>('autoExportCompileCommands', true)) {
        void exportCompileCommandsSilent();
    }
}

export function deactivate(): void {
    statusBar?.dispose();
}

// ── Helpers ───────────────────────────────────────────────────────────────────

async function runTask(task: string, release: boolean): Promise<void> {
    const cfg    = vscode.workspace.getConfiguration('dotori');
    const target = cfg.get<string>('defaultTarget', '').trim();
    const cfgRel = cfg.get<string>('defaultConfiguration', 'debug') === 'release';
    const useRelease = task === 'build' ? (release || cfgRel) : release;

    const tasks = await vscode.tasks.fetchTasks({ type: DotoriTaskProvider.taskType });

    // Prefer a task matching the current target/config
    let match = tasks.find(t =>
        t.name.includes(task) &&
        (useRelease ? t.name.includes('[release]') : !t.name.includes('[release]'))
    );

    // Fallback: first task matching the command name
    match ??= tasks.find(t => t.name.includes(task));

    if (match) {
        await vscode.tasks.executeTask(match);
    } else {
        // No task discovered — run dotori directly
        const exe = DotoriCli.findExecutable();
        if (!exe) {
            void vscode.window.showErrorMessage(
                'dotori not found. Install dotori and ensure it is in PATH.'
            );
            return;
        }

        const args: string[] = [task, '--all'];
        if (useRelease) { args.push('--release'); }
        if (target)     { args.push('--target', target); }

        const cwd = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
        const terminal = vscode.window.createTerminal({ name: `dotori ${task}`, cwd });
        terminal.sendText([exe, ...args].join(' '));
        terminal.show();
    }
}

async function exportCompileCommands(): Promise<void> {
    const folder = vscode.workspace.workspaceFolders?.[0];
    if (!folder) { return; }

    const cfg     = vscode.workspace.getConfiguration('dotori');
    const target  = cfg.get<string>('defaultTarget', '').trim() || undefined;
    const release = cfg.get<string>('defaultConfiguration', 'debug') === 'release';

    await vscode.window.withProgress(
        { location: vscode.ProgressLocation.Notification, title: 'Dotori: Generating compile_commands.json…', cancellable: false },
        async () => {
            const ok = await DotoriCli.exportCompileCommands(folder.uri.fsPath, target, release);
            if (ok) {
                void vscode.window.showInformationMessage('compile_commands.json updated.');
            } else {
                void vscode.window.showErrorMessage(
                    'dotori export compile-commands failed. Check the Output panel.'
                );
            }
        }
    );
}

async function exportCompileCommandsSilent(): Promise<void> {
    const folder = vscode.workspace.workspaceFolders?.[0];
    if (!folder) { return; }

    const cfg    = vscode.workspace.getConfiguration('dotori');
    const target = cfg.get<string>('defaultTarget', '').trim() || undefined;

    // Fire-and-forget; errors are silently ignored on auto-export
    await DotoriCli.exportCompileCommands(folder.uri.fsPath, target, false);
}

async function selectTarget(): Promise<void> {
    const items = KNOWN_TARGETS.map(t => ({
        label:       t.label.replace('$(chip) ', ''),
        description: t.description,
        iconPath:    new vscode.ThemeIcon('chip'),
    }));

    const pick = await vscode.window.showQuickPick(items, {
        placeHolder: 'Select build target',
        matchOnDescription: true,
    });

    if (pick) {
        const cfg = vscode.workspace.getConfiguration('dotori');
        const value = pick.label === 'auto' ? '' : pick.label;
        await cfg.update('defaultTarget', value, vscode.ConfigurationTarget.Workspace);
        statusBar.update();
    }
}

async function selectConfig(): Promise<void> {
    const cfg     = vscode.workspace.getConfiguration('dotori');
    const current = cfg.get<string>('defaultConfiguration', 'debug');

    const items = [
        { label: 'debug',   description: 'No optimization, full debug info', picked: current === 'debug' },
        { label: 'release', description: 'Optimized, no debug info',         picked: current === 'release' },
    ];

    const pick = await vscode.window.showQuickPick(items, {
        placeHolder: 'Select build configuration',
    });

    if (pick) {
        await cfg.update('defaultConfiguration', pick.label, vscode.ConfigurationTarget.Workspace);
        statusBar.update();
    }
}

function updateWorkspaceContext(): void {
    const has = (vscode.workspace.workspaceFolders ?? []).length > 0;
    void vscode.commands.executeCommand('setContext', 'dotori.isWorkspace', has);
}
