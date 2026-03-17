using System.Text;
using Dotori.LanguageServer.Transport;

namespace Dotori.Tests.LanguageServer;

[TestClass]
public class LspTransportTests
{
    [TestMethod]
    public async Task ReadMessage_ValidFraming_ReturnsBody()
    {
        // Arrange
        const string body = "{\"jsonrpc\":\"2.0\",\"method\":\"initialize\"}";
        var header = $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n\r\n";
        var fullMessage = header + body;
        var inputBytes = Encoding.UTF8.GetBytes(fullMessage);

        using var input  = new MemoryStream(inputBytes);
        using var output = new MemoryStream();
        var transport = new LspTransport(input, output);

        // Act
        var result = await transport.ReadMessageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(body, result);
    }

    [TestMethod]
    public async Task WriteMessage_ProducesCorrectFraming()
    {
        // Arrange
        using var input  = new MemoryStream();
        using var output = new MemoryStream();
        var transport = new LspTransport(input, output);
        const string body = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{}}";

        // Act
        await transport.WriteMessageAsync(body, CancellationToken.None);

        // Assert
        output.Seek(0, SeekOrigin.Begin);
        var written = Encoding.UTF8.GetString(output.ToArray());
        var expectedLen = Encoding.UTF8.GetByteCount(body);
        Assert.StartsWith($"Content-Length: {expectedLen}\r\n\r\n", written);
        Assert.EndsWith(body, written);
    }

    [TestMethod]
    public async Task ReadMessage_MultipleMessages_ReadsEachSeparately()
    {
        // Arrange
        var msg1 = "{\"jsonrpc\":\"2.0\",\"method\":\"initialize\"}";
        var msg2 = "{\"jsonrpc\":\"2.0\",\"method\":\"shutdown\"}";
        var frame1 = $"Content-Length: {Encoding.UTF8.GetByteCount(msg1)}\r\n\r\n{msg1}";
        var frame2 = $"Content-Length: {Encoding.UTF8.GetByteCount(msg2)}\r\n\r\n{msg2}";
        var bytes = Encoding.UTF8.GetBytes(frame1 + frame2);

        using var input  = new MemoryStream(bytes);
        using var output = new MemoryStream();
        var transport = new LspTransport(input, output);

        // Act
        var r1 = await transport.ReadMessageAsync(CancellationToken.None);
        var r2 = await transport.ReadMessageAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(msg1, r1);
        Assert.AreEqual(msg2, r2);
    }

    [TestMethod]
    public async Task ReadMessage_EofStream_ReturnsNull()
    {
        // Arrange — empty stream
        using var input  = new MemoryStream(Array.Empty<byte>());
        using var output = new MemoryStream();
        var transport = new LspTransport(input, output);

        // Act
        var result = await transport.ReadMessageAsync(CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task WriteAndRead_RoundTrip_PreservesBody()
    {
        // Use a piped MemoryStream: write to output, then read from same bytes
        const string body = "{\"hello\":\"world\",\"unicode\":\"C++\"}";
        using var output = new MemoryStream();
        using var dummyInput = new MemoryStream();
        var writeTransport = new LspTransport(dummyInput, output);

        await writeTransport.WriteMessageAsync(body, CancellationToken.None);

        // Now read it back
        output.Seek(0, SeekOrigin.Begin);
        using var dummyOutput = new MemoryStream();
        var readTransport = new LspTransport(output, dummyOutput);
        var result = await readTransport.ReadMessageAsync(CancellationToken.None);

        Assert.AreEqual(body, result);
    }
}
