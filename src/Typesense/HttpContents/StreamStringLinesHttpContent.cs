using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Typesense.HttpContents;

public sealed class StreamStringLinesHttpContent : System.Net.Http.HttpContent
{
    private readonly IEnumerable<string> _lines;

    public StreamStringLinesHttpContent(IEnumerable<string> lines)
    {
        _lines = lines ?? throw new ArgumentNullException(nameof(lines));
    }
    protected override bool TryComputeLength(out long length)
    {
        // This content doesn't support pre-computed length and
        // the request will NOT contain Content-Length header.
        // It defeats the purpose of streaming the lines
        length = 0;
        return false;
    }

    // SerializeToStream* methods are internally used by CopyTo* methods
    // which in turn are used to copy the content to the NetworkStream.
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return SerializeToStreamAsync(stream, context, cancellationToken: default);
    }

    // Override SerializeToStreamAsync overload with CancellationToken
    // if the content serialization supports cancellation, otherwise the token will be dropped.
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        await using StreamWriter streamWriter = new(stream, leaveOpen: true);
        foreach (var line in _lines)
            await streamWriter.WriteLineAsync(line).ConfigureAwait(false);
    }

    // In rare cases when synchronous support is needed, e.g. synchronous CopyTo used by HttpClient.Send,
    // implement synchronous version of SerializeToStream.
    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        using StreamWriter streamWriter = new(stream, leaveOpen: true);
        foreach (var line in _lines)
            streamWriter.WriteLine(line);
    }

    // CreateContentReadStream* methods, if implemented, will be used by ReadAsStream* methods
    // to get the underlying stream and avoid buffering.
    // These methods will not be used by HttpClient on a custom content.
    // They are for content receiving and HttpClient uses its own internal implementation for an HTTP response content.
    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        return CreateContentReadStreamAsync(cancellationToken: default);
    }

    // Override CreateContentReadStreamAsync overload with CancellationToken
    // if the content serialization supports cancellation, otherwise the token will be dropped.
    protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(CreateContentReadStream(cancellationToken));
    }

    // In rare cases when synchronous support is needed, e.g. synchronous ReadAsStream,
    // implement synchronous version of CreateContentRead.
    protected override Stream CreateContentReadStream(CancellationToken cancellationToken)
    {
        MemoryStream stream = new();
        SerializeToStream(stream, context: null, cancellationToken);
        return stream;
    }
}