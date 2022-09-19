namespace CatenaX.NetworkServices.Framework.IO;

public sealed class CancellableStream : Stream
{
    public CancellableStream(Stream stream, CancellationToken cancellationToken) : base()
    {
        _stream = stream;
        _cancellationToken = cancellationToken;
    }

    private readonly CancellationToken _cancellationToken;
    private readonly Stream _stream;

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => false;
    public override bool CanTimeout => _stream.CanTimeout;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get { throw new NotSupportedException(); }
        set { throw new NotSupportedException(); }
    }
    public override long Seek (long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte [] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte [] buffer, int offset, int count) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        _stream.ReadAsync(buffer, cancellationToken == default ? _cancellationToken : cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
        _stream.WriteAsync(buffer, cancellationToken == default ? _cancellationToken : cancellationToken);
}
