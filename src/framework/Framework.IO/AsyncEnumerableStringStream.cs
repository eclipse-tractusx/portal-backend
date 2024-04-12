/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
public sealed class AsyncEnumerableStringStream : Stream
{
    public AsyncEnumerableStringStream(IAsyncEnumerable<string> data, Encoding encoding) : base()
    {
        _enumerator = data.GetAsyncEnumerator();
        _stream = new MemoryStream();
        _writer = new StreamWriter(_stream, encoding);
    }

    private readonly IAsyncEnumerator<string> _enumerator;
    private readonly MemoryStream _stream;
    private readonly TextWriter _writer;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanTimeout => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get { throw new NotSupportedException(); }
        set { throw new NotSupportedException(); }
    }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var written = _stream.Read(buffer.Span);
        while (buffer.Length - written > 0 && await _enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
        {
            _stream.Position = 0;
            _stream.SetLength(0);
            _writer.WriteLine(_enumerator.Current);
            _writer.Flush();
            _stream.Position = 0;

            written += _stream.Read(buffer.Span.Slice(written));
        }
        return written;
    }
}
