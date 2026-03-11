import * as vscode from 'vscode';

/**
 * Manages the dotori status bar item.
 *
 * Shows:  $(tools) dotori: <target> [<config>]
 * Click:  quick-pick to switch debug/release
 */
export class DotoriStatusBar {

    private readonly item: vscode.StatusBarItem;

    constructor(context: vscode.ExtensionContext) {
        this.item = vscode.window.createStatusBarItem(
            'dotori.statusBar',
            vscode.StatusBarAlignment.Right,
            100
        );
        this.item.command = 'dotori.selectConfig';
        this.item.tooltip = 'Dotori: Click to change build configuration';
        context.subscriptions.push(this.item);
    }

    update(): void {
        const cfg    = vscode.workspace.getConfiguration('dotori');
        const show   = cfg.get<boolean>('showStatusBar', true);
        const target = cfg.get<string>('defaultTarget', '').trim() || 'auto';
        const config = cfg.get<string>('defaultConfiguration', 'debug');

        if (!show) {
            this.item.hide();
            return;
        }

        this.item.text    = `$(tools) dotori: ${target} [${config}]`;
        this.item.backgroundColor = config === 'release'
            ? new vscode.ThemeColor('statusBarItem.warningBackground')
            : undefined;
        this.item.show();
    }

    dispose(): void {
        this.item.dispose();
    }
}
