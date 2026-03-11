import * as vscode from 'vscode';
import { DotoriCli } from './cli';

/**
 * Watches .dotori files in the workspace and re-generates compile_commands.json
 * whenever they are created, changed, or deleted — keeping clangd IntelliSense
 * up to date automatically.
 */
export class DotoriFileWatcher implements vscode.Disposable {

    private readonly _watcher: vscode.FileSystemWatcher;
    private _debounceTimer: ReturnType<typeof setTimeout> | undefined;
    private readonly _debounceMs = 1500;

    constructor(context: vscode.ExtensionContext) {
        // Watch every .dotori file anywhere in the workspace
        this._watcher = vscode.workspace.createFileSystemWatcher('**/.dotori');

        this._watcher.onDidChange(uri => this._schedule(uri, 'changed'),  undefined, context.subscriptions);
        this._watcher.onDidCreate(uri => this._schedule(uri, 'created'),  undefined, context.subscriptions);
        this._watcher.onDidDelete(uri => this._schedule(uri, 'deleted'),  undefined, context.subscriptions);

        context.subscriptions.push(this);
    }

    dispose(): void {
        if (this._debounceTimer !== undefined) {
            clearTimeout(this._debounceTimer);
        }
        this._watcher.dispose();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private _schedule(_uri: vscode.Uri, _reason: string): void {
        const cfg = vscode.workspace.getConfiguration('dotori');
        if (!cfg.get<boolean>('autoExportCompileCommands', true)) { return; }

        // Debounce: if several files change rapidly (e.g. git checkout), wait
        if (this._debounceTimer !== undefined) {
            clearTimeout(this._debounceTimer);
        }
        this._debounceTimer = setTimeout(() => {
            this._debounceTimer = undefined;
            void this._regenerate();
        }, this._debounceMs);
    }

    private async _regenerate(): Promise<void> {
        const folder = vscode.workspace.workspaceFolders?.[0];
        if (!folder) { return; }

        const cfg    = vscode.workspace.getConfiguration('dotori');
        const target = cfg.get<string>('defaultTarget', '').trim() || undefined;

        // Silent regeneration — errors are shown as a status bar warning only
        const ok = await DotoriCli.exportCompileCommands(folder.uri.fsPath, target, false);
        if (!ok) {
            // Show a brief warning in the status bar without an intrusive popup
            void vscode.window.setStatusBarMessage(
                '$(warning) Dotori: compile_commands.json update failed',
                5000
            );
        }
    }
}
