import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { DotoriCli } from './cli';

// ─── Tree item types ──────────────────────────────────────────────────────────

class ProjectItem extends vscode.TreeItem {
    constructor(
        public readonly projectName: string,
        public readonly dotoriPath:  string,
        public readonly deps:        string[],
        public readonly level:       number,
    ) {
        super(
            projectName,
            deps.length > 0
                ? vscode.TreeItemCollapsibleState.Collapsed
                : vscode.TreeItemCollapsibleState.None,
        );

        this.description = path.relative(
            vscode.workspace.workspaceFolders?.[0]?.uri.fsPath ?? '',
            path.dirname(dotoriPath),
        ) || '.';

        this.tooltip = new vscode.MarkdownString(
            `**${projectName}**\n\n` +
            `Path: \`${dotoriPath}\`\n\n` +
            (deps.length > 0
                ? `Depends on: ${deps.map(d => `\`${d}\``).join(', ')}`
                : 'No dependencies'),
        );

        this.iconPath  = new vscode.ThemeIcon('package');
        this.contextValue = 'dotoriProject';

        this.command = {
            command:   'vscode.open',
            title:     'Open .dotori',
            arguments: [vscode.Uri.file(dotoriPath)],
        };
    }
}

class DependencyEdgeItem extends vscode.TreeItem {
    constructor(public readonly depName: string) {
        super(depName, vscode.TreeItemCollapsibleState.None);
        this.iconPath    = new vscode.ThemeIcon('arrow-right');
        this.contextValue = 'dotoriDep';
    }
}

type AnyItem = ProjectItem | DependencyEdgeItem;

// ─── Provider ─────────────────────────────────────────────────────────────────

/**
 * Shows the dotori project dependency DAG in a VS Code Tree View.
 * Calls `dotori graph --all` to discover the project graph.
 */
export class DotoriProjectTreeProvider
    implements vscode.TreeDataProvider<AnyItem>
{
    private _onDidChangeTreeData =
        new vscode.EventEmitter<AnyItem | undefined | void>();
    readonly onDidChangeTreeData = this._onDidChangeTreeData.event;

    /** name → ProjectItem, populated after refresh */
    private _projects: ProjectItem[] = [];
    private _byName = new Map<string, ProjectItem>();

    constructor(context: vscode.ExtensionContext) {
        context.subscriptions.push(
            vscode.commands.registerCommand('dotori.refreshTree', () => this.refresh()),
        );

        // Refresh when .dotori files change
        const watcher = vscode.workspace.createFileSystemWatcher('**/.dotori');
        context.subscriptions.push(
            watcher,
            watcher.onDidChange(() => this.refresh()),
            watcher.onDidCreate(() => this.refresh()),
            watcher.onDidDelete(() => this.refresh()),
        );
    }

    refresh(): void {
        this._projects = [];
        this._byName.clear();
        this._onDidChangeTreeData.fire();
    }

    getTreeItem(element: AnyItem): vscode.TreeItem {
        return element;
    }

    async getChildren(element?: AnyItem): Promise<AnyItem[]> {
        if (element instanceof DependencyEdgeItem) {
            return [];
        }

        if (element instanceof ProjectItem) {
            // Show the dep edges as children of a project node
            return element.deps.map(d => new DependencyEdgeItem(d));
        }

        // Root: load projects from workspace
        await this._ensureLoaded();
        return this._projects;
    }

    // ─── Loading ──────────────────────────────────────────────────────────────

    private async _ensureLoaded(): Promise<void> {
        if (this._projects.length > 0) { return; }
        await this._loadProjects();
    }

    private async _loadProjects(): Promise<void> {
        const folder = vscode.workspace.workspaceFolders?.[0];
        if (!folder) { return; }

        // Try `dotori info graph --all` to get the DAG
        const result = await DotoriCli.run(
            ['info', 'graph', '--all'],
            folder.uri.fsPath,
            15_000,
        );

        if (result.exitCode === 0 && result.stdout.trim()) {
            this._parseGraphOutput(result.stdout, folder.uri.fsPath);
            return;
        }

        // Fallback: scan workspace for .dotori files directly
        this._scanWorkspace(folder.uri.fsPath);
    }

    /**
     * Parse the output of `dotori info graph`:
     *
     * ```
     * Project dependency graph (build order):
     *
     *   MyApp → depends on: MyLib, Core
     *   MyLib → depends on: Core
     *   Core
     * ```
     */
    private _parseGraphOutput(output: string, workspaceRoot: string): void {
        const lines = output.split('\n');
        const items: ProjectItem[] = [];
        const allDotoriFiles = this._findDotoriFiles(workspaceRoot);

        for (const line of lines) {
            const m = /^\s{2}([A-Za-z_][A-Za-z0-9_\-]*)(?:\s+→\s+depends on:\s*(.+))?$/.exec(line);
            if (!m) { continue; }

            const name      = m[1];
            const depsStr   = m[2]?.trim() ?? '';
            const deps      = depsStr ? depsStr.split(',').map(d => d.trim()).filter(Boolean) : [];
            const dotoriPath = this._findDotoriForName(name, allDotoriFiles) ?? workspaceRoot;

            const item = new ProjectItem(name, dotoriPath, deps, 0);
            items.push(item);
            this._byName.set(name, item);
        }

        this._projects = items;
    }

    /** Find all .dotori files in the workspace (up to 200). */
    private _findDotoriFiles(root: string): string[] {
        const results: string[] = [];
        this._walkDir(root, results, 5, 200);
        return results;
    }

    private _walkDir(dir: string, out: string[], maxDepth: number, limit: number): void {
        if (maxDepth < 0 || out.length >= limit) { return; }
        let entries: string[];
        try { entries = fs.readdirSync(dir); } catch { return; }

        for (const entry of entries) {
            if (entry.startsWith('.') && entry !== '.dotori') { continue; }
            const full = path.join(dir, entry);
            if (entry === '.dotori') {
                out.push(full);
            } else {
                try {
                    if (fs.statSync(full).isDirectory() &&
                        entry !== 'node_modules' &&
                        entry !== '.dotori-cache')
                    {
                        this._walkDir(full, out, maxDepth - 1, limit);
                    }
                } catch { /* skip */ }
            }
        }
    }

    private _findDotoriForName(name: string, files: string[]): string | undefined {
        for (const f of files) {
            try {
                const text = fs.readFileSync(f, 'utf8');
                const m = /\bproject\s+([A-Za-z_][A-Za-z0-9_\-]*)\s*\{/.exec(text);
                if (m && m[1] === name) { return f; }
            } catch { /* skip */ }
        }
        return undefined;
    }

    /** Fallback: scan workspace for .dotori files and create simple nodes. */
    private _scanWorkspace(root: string): void {
        const files = this._findDotoriFiles(root);
        for (const f of files) {
            try {
                const text = fs.readFileSync(f, 'utf8');
                const m = /\bproject\s+([A-Za-z_][A-Za-z0-9_\-]*)\s*\{/.exec(text);
                if (m) {
                    const item = new ProjectItem(m[1], f, [], 0);
                    this._projects.push(item);
                    this._byName.set(m[1], item);
                }
            } catch { /* skip */ }
        }
    }
}
