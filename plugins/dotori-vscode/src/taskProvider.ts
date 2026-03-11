import * as vscode from 'vscode';
import * as path from 'path';
import { DotoriCli } from './cli';

export interface DotoriTaskDefinition extends vscode.TaskDefinition {
    task: string;         // 'build' | 'run' | 'clean' | 'test' | 'export-compile-commands'
    project?: string;     // path to .dotori file or directory
    release?: boolean;
    target?: string;
    args?: string[];
}

/**
 * TaskProvider for dotori.
 * Auto-discovers .dotori files and exposes Build / Run / Clean tasks.
 */
export class DotoriTaskProvider implements vscode.TaskProvider {

    static readonly taskType = 'dotori';

    async provideTasks(_token: vscode.CancellationToken): Promise<vscode.Task[]> {
        const tasks: vscode.Task[] = [];

        if (!vscode.workspace.workspaceFolders) {
            return tasks;
        }

        for (const folder of vscode.workspace.workspaceFolders) {
            const files = await vscode.workspace.findFiles(
                new vscode.RelativePattern(folder, '**/.dotori'),
                '**/.dotori-cache/**',
                50
            );

            for (const uri of files) {
                const raw = await vscode.workspace.fs.readFile(uri);
                const text = Buffer.from(raw).toString('utf8');
                const name = DotoriCli.extractProjectName(text) ?? path.basename(path.dirname(uri.fsPath));

                tasks.push(this.makeTask(folder, name, uri.fsPath, 'build',  false, vscode.TaskGroup.Build));
                tasks.push(this.makeTask(folder, name, uri.fsPath, 'build',  true,  vscode.TaskGroup.Build));
                tasks.push(this.makeTask(folder, name, uri.fsPath, 'run',    false));
                tasks.push(this.makeTask(folder, name, uri.fsPath, 'run',    true));
                tasks.push(this.makeTask(folder, name, uri.fsPath, 'clean',  false, vscode.TaskGroup.Clean));
                tasks.push(this.makeTask(folder, name, uri.fsPath, 'test',   false, vscode.TaskGroup.Test));
            }
        }

        return tasks;
    }

    resolveTask(task: vscode.Task, _token: vscode.CancellationToken): vscode.Task | undefined {
        const def = task.definition as DotoriTaskDefinition;
        if (def.type !== DotoriTaskProvider.taskType) {
            return undefined;
        }
        return this.resolveDefinition(def, task.scope as vscode.WorkspaceFolder);
    }

    private makeTask(
        folder: vscode.WorkspaceFolder,
        projectName: string,
        dotoriPath: string,
        taskName: string,
        release: boolean,
        group?: vscode.TaskGroup
    ): vscode.Task {
        const def: DotoriTaskDefinition = {
            type: DotoriTaskProvider.taskType,
            task: taskName,
            project: dotoriPath,
            release,
        };

        const label = release
            ? `${projectName}: ${taskName} [release]`
            : `${projectName}: ${taskName}`;

        const task = this.resolveDefinition(def, folder, label);
        if (group) { task.group = group; }
        task.presentationOptions = {
            reveal: vscode.TaskRevealKind.Always,
            panel: vscode.TaskPanelKind.Shared,
            showReuseMessage: false,
            clear: false,
        };
        return task;
    }

    private resolveDefinition(
        def: DotoriTaskDefinition,
        scope: vscode.WorkspaceFolder | vscode.TaskScope | undefined,
        label?: string
    ): vscode.Task {
        const cfg     = vscode.workspace.getConfiguration('dotori');
        const cfgTarget = cfg.get<string>('defaultTarget', '').trim();

        const args: string[] = [def.task];
        if (def.project) { args.push('--project', def.project); }
        if (def.release)  { args.push('--release'); }
        if (def.target ?? cfgTarget) { args.push('--target', (def.target ?? cfgTarget)!); }
        if (def.args)     { args.push(...def.args); }

        const exe = DotoriCli.findExecutable() ?? 'dotori';
        const execution = new vscode.ShellExecution(
            exe,
            args,
            { cwd: def.project ? path.dirname(def.project) : undefined }
        );

        const resolvedLabel = label ?? def.task;
        return new vscode.Task(
            def,
            scope ?? vscode.TaskScope.Workspace,
            resolvedLabel,
            'dotori',
            execution,
            '$gcc' // problem matcher for compiler output
        );
    }
}
