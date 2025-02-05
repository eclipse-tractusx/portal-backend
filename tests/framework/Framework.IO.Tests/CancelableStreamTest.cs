/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO.Tests;

public class CancelableStreamTest
{
    private readonly Stream _stream;
    private readonly Memory<byte> _memory;
    private readonly int _numBytesFirstRead;
    private readonly int _numBytesSecondRead;
    private readonly Func<int> _getNumBytes;
    private readonly OperationCanceledException _sourceTokenCanceledException;
    private readonly OperationCanceledException _otherTokenCanceledException;

    public CancelableStreamTest()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _stream = A.Fake<Stream>();
        _memory = fixture.Create<Memory<byte>>();
        _numBytesFirstRead = fixture.Create<int>();
        _numBytesSecondRead = fixture.Create<int>();
        _getNumBytes = A.Fake<Func<int>>();
        _sourceTokenCanceledException = fixture.Create<OperationCanceledException>();
        _otherTokenCanceledException = fixture.Create<OperationCanceledException>();
    }

    #region Tests

    [Fact]
    public async Task TestReadAsyncNoOtherTokenSuccess()
    {
        var (sut, source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory);
        var secondResult = await sut.ReadAsync(_memory);
        firstResult.Should().Be(_numBytesFirstRead);
        secondResult.Should().Be(_numBytesSecondRead);
        source.Dispose();
    }

    [Fact]
    public async Task TestReadAsyncWithUncancelledOtherTokenSuccess()
    {
        var (sut, source) = SetupForRead();
        using var firstSource = new CancellationTokenSource();
        using var secondSource = new CancellationTokenSource();
        var firstResult = await sut.ReadAsync(_memory, firstSource.Token);
        var secondResult = await sut.ReadAsync(_memory, secondSource.Token);
        firstResult.Should().Be(_numBytesFirstRead);
        secondResult.Should().Be(_numBytesSecondRead);
        source.Dispose();
    }

    [Fact]
    public async Task TestReadAsyncSourceCancelledWithUncancelledOtherTokenSuccess()
    {
        var (sut, source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory);
        source.Cancel();
        using var secondSource = new CancellationTokenSource();
        var secondResult = await sut.ReadAsync(_memory, secondSource.Token);
        firstResult.Should().Be(_numBytesFirstRead);
        secondResult.Should().Be(_numBytesSecondRead);
        source.Dispose();
    }

    [Fact]
    public async Task TestReadAsyncWithCancelledOtherTokenThrows()
    {
        var (sut, source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory);
        firstResult.Should().Be(_numBytesFirstRead);

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await sut.ReadExactlyAsync(_memory, new CancellationToken(true))
        );
        exception.Should().Be(_otherTokenCanceledException);
        source.Dispose();
    }

    [Fact]
    public async Task TestReadAsyncSourceCancelledThrows()
    {
        var (sut, source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory);
        firstResult.Should().Be(_numBytesFirstRead);

        source.Cancel();
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await sut.ReadExactlyAsync(_memory)
        );
        exception.Should().Be(_sourceTokenCanceledException);
        source.Dispose();
    }

    #endregion

    #region Setup

    private (Stream, CancellationTokenSource) SetupForRead()
    {
        var source = new CancellationTokenSource();
        A.CallTo(() => _getNumBytes()).ReturnsNextFromSequence(_numBytesFirstRead, _numBytesSecondRead);
        A.CallTo(() => _stream.CanRead).Returns(true);
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.That.IsNotCanceled())).ReturnsLazily(() => _getNumBytes());
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.That.Matches(t => t == source.Token && t.CanBeCanceled && t.IsCancellationRequested))).Throws(_sourceTokenCanceledException);
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.That.Matches(t => t != source.Token && t.CanBeCanceled && t.IsCancellationRequested))).Throws(_otherTokenCanceledException);
        return (new CancellableStream(_stream, source.Token), source);
    }

    #endregion
}
