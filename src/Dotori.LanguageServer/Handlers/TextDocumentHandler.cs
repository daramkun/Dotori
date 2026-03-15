using Dotori.LanguageServer.Protocol;
using Dotori.LanguageServer.Providers;

namespace Dotori.LanguageServer.Handlers;

/// <summary>
/// Handles textDocument/didOpen, didChange, didClose lifecycle,
/// and triggers diagnostics publishing.
/// </summary>
public sealed class TextDocumentHandler
{
    private readonly DocumentStore _store;
    private readonly Action<string, List<LspDiagnostic>> _publishDiagnostics;

    public TextDocumentHandler(
        DocumentStore store,
        Action<string, List<LspDiagnostic>> publishDiagnostics)
    {
        _store = store;
        _publishDiagnostics = publishDiagnostics;
    }

    public void HandleDidOpen(DidOpenTextDocumentParams p)
    {
        _store.Open(p.TextDocument.Uri, p.TextDocument.Text, p.TextDocument.Version);
        AnalyzeAndPublish(p.TextDocument.Uri, p.TextDocument.Text);
    }

    public void HandleDidChange(DidChangeTextDocumentParams p)
    {
        if (p.ContentChanges.Count == 0) return;
        var text = p.ContentChanges[^1].Text; // Full sync: take last
        _store.Update(p.TextDocument.Uri, text, p.TextDocument.Version);
        AnalyzeAndPublish(p.TextDocument.Uri, text);
    }

    public void HandleDidClose(DidCloseTextDocumentParams p)
    {
        _store.Close(p.TextDocument.Uri);
        // Clear diagnostics on close
        _publishDiagnostics(p.TextDocument.Uri, []);
    }

    private void AnalyzeAndPublish(string uri, string text)
    {
        var doc = _store.Get(uri);
        var filePath = doc?.LocalPath ?? uri;
        var diagnostics = DiagnosticsProvider.Analyze(text, filePath);
        _publishDiagnostics(uri, diagnostics);
    }
}
