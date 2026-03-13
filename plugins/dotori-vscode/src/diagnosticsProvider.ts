import * as vscode from 'vscode';
import * as path from 'path';
import { DotoriCli } from './cli';

/**
 * Provides real-time error diagnostics for `.dotori` files by running
 * `dotori check` and parsing its output.
 *
 * Parse errors are reported with exact file / line / column when available.
 * The format emitted by `dotori check` is:
 *   Error in '<file>': <message>  (with optional " at <line>:<col>")
 *   OR a general "Parse error: <file>:<line>:<col>: <message>" on stderr.
 */
export class DotoriDiagnosticsProvider {

    private readonly _collection: vscode.DiagnosticCollection;
    private readonly _debounceMap = new Map<string, ReturnType<typeof setTimeout>>();
    private readonly _DEBOUNCE_MS = 600;

    constructor(context: vscode.ExtensionContext) {
        this._collection = vscode.languages.createDiagnosticCollection('dotori');
        context.subscriptions.push(this._collection);

        // Validate open documents on activation
        for (const doc of vscode.workspace.textDocuments) {
            if (this._isDotori(doc)) {
                void this._validateDebounced(doc.uri);
            }
        }

        context.subscriptions.push(
            vscode.workspace.onDidOpenTextDocument(doc => {
                if (this._isDotori(doc)) { void this._validateDebounced(doc.uri); }
            }),
            vscode.workspace.onDidChangeTextDocument(e => {
                if (this._isDotori(e.document)) { void this._validateDebounced(e.document.uri); }
            }),
            vscode.workspace.onDidSaveTextDocument(doc => {
                if (this._isDotori(doc)) { void this._validateDebounced(doc.uri); }
            }),
            vscode.workspace.onDidCloseTextDocument(doc => {
                this._collection.delete(doc.uri);
                this._debounceMap.delete(doc.uri.toString());
            }),
        );
    }

    private _isDotori(doc: vscode.TextDocument): boolean {
        return doc.languageId === 'dotori' || doc.uri.fsPath.endsWith('.dotori');
    }

    private _validateDebounced(uri: vscode.Uri): void {
        const key = uri.toString();
        const existing = this._debounceMap.get(key);
        if (existing) { clearTimeout(existing); }
        const timer = setTimeout(() => {
            this._debounceMap.delete(key);
            void this._validate(uri);
        }, this._DEBOUNCE_MS);
        this._collection.delete(uri);  // clear stale diags immediately
        this._debounceMap.set(key, timer);
    }

    private async _validate(uri: vscode.Uri): Promise<void> {
        const filePath  = uri.fsPath;
        const cwd       = path.dirname(filePath);

        const result = await DotoriCli.run(
            ['check', '--project', filePath],
            cwd,
            10_000,
        );

        const diagnostics: vscode.Diagnostic[] = [];

        // Parse combined stdout + stderr for error lines
        const output = result.stdout + '\n' + result.stderr;
        for (const line of output.split('\n')) {
            const diag = this._parseLine(line, filePath);
            if (diag) { diagnostics.push(diag); }
        }

        this._collection.set(uri, diagnostics);
    }

    /**
     * Attempt to parse a single output line into a VS Code Diagnostic.
     *
     * Recognised patterns:
     *   1. `Parse error: <file>:<line>:<col>: <message>`
     *   2. `  Error in '<file>': <message>` (with optional ` at <line>:<col>`)
     *   3. `  Warning: '<file>' line <line>: <message>`
     */
    private _parseLine(
        line: string,
        _targetFile: string,
    ): vscode.Diagnostic | undefined {
        line = line.trim();
        if (!line) { return undefined; }

        // Pattern 1 — "Parse error: path:line:col: message"
        const p1 = /^Parse error:\s*(.+?):(\d+):(\d+):\s*(.+)$/.exec(line);
        if (p1) {
            const [, , lineStr, colStr, msg] = p1;
            return this._makeDiag(
                parseInt(lineStr, 10) - 1,
                parseInt(colStr, 10)  - 1,
                msg,
                vscode.DiagnosticSeverity.Error,
            );
        }

        // Pattern 2 — "Error in '<file>': message [at line:col]"
        const p2 = /^Error\s+in\s+'.+?':\s*(.+?)(?:\s+at\s+(\d+):(\d+))?$/.exec(line);
        if (p2) {
            const [, msg, lineStr, colStr] = p2;
            const l = lineStr ? parseInt(lineStr, 10) - 1 : 0;
            const c = colStr  ? parseInt(colStr,  10) - 1 : 0;
            return this._makeDiag(l, c, msg, vscode.DiagnosticSeverity.Error);
        }

        // Pattern 3 — "Warning: '<file>' line <N>: message"
        const p3 = /^Warning:\s*'.+?'\s+line\s+(\d+):\s*(.+)$/.exec(line);
        if (p3) {
            const [, lineStr, msg] = p3;
            return this._makeDiag(
                parseInt(lineStr, 10) - 1,
                0,
                msg,
                vscode.DiagnosticSeverity.Warning,
            );
        }

        return undefined;
    }

    private _makeDiag(
        line0: number,
        col0:  number,
        message: string,
        severity: vscode.DiagnosticSeverity,
    ): vscode.Diagnostic {
        const l   = Math.max(0, line0);
        const c   = Math.max(0, col0);
        const pos = new vscode.Position(l, c);
        return new vscode.Diagnostic(
            new vscode.Range(pos, pos),
            message,
            severity,
        );
    }
}
