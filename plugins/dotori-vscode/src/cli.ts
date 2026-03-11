import * as cp from 'child_process';
import * as path from 'path';
import * as fs from 'fs';
import * as vscode from 'vscode';

export interface CliResult {
    exitCode: number;
    stdout: string;
    stderr: string;
}

/**
 * Finds and invokes the dotori CLI executable.
 */
export class DotoriCli {

    /**
     * Locate the dotori executable.
     * Checks the user setting first, then PATH.
     */
    static findExecutable(): string | undefined {
        const cfg = vscode.workspace.getConfiguration('dotori');
        const configured = cfg.get<string>('executablePath', '').trim();
        if (configured) {
            return fs.existsSync(configured) ? configured : undefined;
        }

        const isWin = process.platform === 'win32';
        const names = isWin
            ? ['dotori.exe', 'dotori.cmd', 'dotori']
            : ['dotori'];

        const pathEnv = process.env['PATH'] ?? '';
        for (const dir of pathEnv.split(path.delimiter)) {
            for (const name of names) {
                const full = path.join(dir, name);
                if (fs.existsSync(full)) {
                    return full;
                }
            }
        }
        return undefined;
    }

    /**
     * Run dotori with given arguments, returning stdout/stderr.
     */
    static run(
        args: string[],
        cwd?: string,
        timeoutMs = 60_000
    ): Promise<CliResult> {
        return new Promise(resolve => {
            const exe = this.findExecutable();
            if (!exe) {
                resolve({ exitCode: -1, stdout: '', stderr: 'dotori not found in PATH' });
                return;
            }

            const proc = cp.spawn(exe, args, {
                cwd,
                shell: false,
                timeout: timeoutMs,
            });

            let stdout = '';
            let stderr = '';
            proc.stdout.on('data', (d: Buffer) => { stdout += d.toString(); });
            proc.stderr.on('data', (d: Buffer) => { stderr += d.toString(); });

            proc.on('close', code => {
                resolve({ exitCode: code ?? -1, stdout, stderr });
            });
            proc.on('error', err => {
                resolve({ exitCode: -1, stdout: '', stderr: err.message });
            });
        });
    }

    /**
     * Run `dotori export compile-commands` for all projects in cwd.
     */
    static async exportCompileCommands(
        cwd: string,
        target?: string,
        release = false
    ): Promise<boolean> {
        const args = ['export', 'compile-commands', '--all'];
        if (target) { args.push('--target', target); }
        if (release) { args.push('--release'); }

        const result = await this.run(args, cwd, 120_000);
        return result.exitCode === 0;
    }

    /**
     * Scan a .dotori file and extract the project name (best-effort).
     */
    static extractProjectName(content: string): string | undefined {
        const m = content.match(/\bproject\s+([a-zA-Z_][a-zA-Z0-9_\-]*)\s*\{/);
        return m?.[1];
    }
}
