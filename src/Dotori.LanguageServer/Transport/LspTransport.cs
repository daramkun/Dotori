using System.Text;

namespace Dotori.LanguageServer.Transport;

/// <summary>
/// Handles the LSP Content-Length framing protocol over stdio.
/// Messages are framed as:
///   Content-Length: &lt;n&gt;\r\n
///   \r\n
///   &lt;json body, n bytes UTF-8&gt;
/// </summary>
public sealed class LspTransport
{
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly TextWriter? _logWriter;

    public LspTransport(Stream input, Stream output, TextWriter? logWriter = null)
    {
        _input = input;
        _output = output;
        _logWriter = logWriter;
    }

    /// <summary>
    /// Read a single LSP message from stdin.
    /// Returns null when the stream ends (client disconnected).
    /// </summary>
    public async Task<string?> ReadMessageAsync(CancellationToken ct)
    {
        // Read headers until blank line
        int? contentLength = null;
        while (true)
        {
            var line = await ReadLineAsync(ct);
            if (line is null) return null;  // EOF
            if (line.Length == 0) break;    // blank line = end of headers

            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                var valueStr = line["Content-Length:".Length..].Trim();
                if (int.TryParse(valueStr, out var len))
                    contentLength = len;
            }
            // ignore other headers (e.g. Content-Type)
        }

        if (contentLength is null)
        {
            _logWriter?.WriteLine("[LSP] Warning: message without Content-Length header");
            return null;
        }

        // Read body
        var buffer = new byte[contentLength.Value];
        var read = 0;
        while (read < contentLength.Value)
        {
            var n = await _input.ReadAsync(buffer.AsMemory(read, contentLength.Value - read), ct);
            if (n == 0) return null; // EOF
            read += n;
        }

        var json = Encoding.UTF8.GetString(buffer);
        _logWriter?.WriteLine($"[LSP] <<< {json}");
        return json;
    }

    /// <summary>
    /// Write a single LSP message to stdout (Content-Length framed).
    /// </summary>
    public async Task WriteMessageAsync(string json, CancellationToken ct)
    {
        _logWriter?.WriteLine($"[LSP] >>> {json}");

        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");

        await _output.WriteAsync(header, ct);
        await _output.WriteAsync(body, ct);
        await _output.FlushAsync(ct);
    }

    private async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        var sb = new StringBuilder();
        while (true)
        {
            var b = new byte[1];
            var n = await _input.ReadAsync(b.AsMemory(0, 1), ct);
            if (n == 0) return sb.Length > 0 ? sb.ToString() : null;

            var c = (char)b[0];
            if (c == '\n')
            {
                // Remove trailing \r if present
                if (sb.Length > 0 && sb[^1] == '\r')
                    sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }
            sb.Append(c);
        }
    }
}
