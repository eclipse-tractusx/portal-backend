/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO.Tests;

public class CancelableStreamTest
{
    private readonly IFixture _fixture;
    private readonly Stream _stream;
    private readonly Memory<byte> _memory;
    private readonly ReadOnlyMemory<byte> _readOnlyMemory;
    private readonly int _numBytesFirstRead;
    private readonly int _numBytesSecondRead;
    private readonly Func<int> _getNumBytes;
    private readonly OperationCanceledException _sourceTokenCanceledException;
    private readonly OperationCanceledException _otherTokenCanceledException;

    public CancelableStreamTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _stream = A.Fake<Stream>();
        _memory = _fixture.Create<Memory<byte>>();
        _readOnlyMemory = _fixture.Create<ReadOnlyMemory<byte>>();
        _numBytesFirstRead = _fixture.Create<int>();
        _numBytesSecondRead = _fixture.Create<int>();
        _getNumBytes = A.Fake<Func<int>>();
        _sourceTokenCanceledException = _fixture.Create<OperationCanceledException>();
        _otherTokenCanceledException = _fixture.Create<OperationCanceledException>();
    }

    #region Tests

    [Fact]
    public async void TestReadAsyncNoOtherTokenSuccess()
    {
        var (sut,source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory).ConfigureAwait(false);
        var secondResult = await sut.ReadAsync(_memory).ConfigureAwait(false);
        firstResult.Should().Be(_numBytesFirstRead);
        secondResult.Should().Be(_numBytesSecondRead);
    }

    [Fact]
    public async void TestReadAsyncWithUncancelledOtherTokenSuccess()
    {
        var (sut,source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory, new CancellationTokenSource().Token).ConfigureAwait(false);
        var secondResult = await sut.ReadAsync(_memory, new CancellationTokenSource().Token).ConfigureAwait(false);
        firstResult.Should().Be(_numBytesFirstRead);
        secondResult.Should().Be(_numBytesSecondRead);
    }

    [Fact]
    public async void TestReadAsyncSourceCancelledWithUncancelledOtherTokenSuccess()
    {
        var (sut,source) = SetupForRead();
        var firstResult = await sut.ReadAsync(_memory).ConfigureAwait(false);
        source.Cancel();
        var secondResult = await sut.ReadAsync(_memory, new CancellationTokenSource().Token).ConfigureAwait(false);
        firstResult.Should().Be(_numBytesFirstRead);
        secondResult.Should().Be(_numBytesSecondRead);
    }

    [Fact]
    public async void TestReadAsyncWithCancelledOtherTokenThrows()
    {
        var (sut,source) = SetupForRead();

        var firstResult = await sut.ReadAsync(_memory).ConfigureAwait(false);
        firstResult.Should().Be(_numBytesFirstRead);

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await sut.ReadAsync(_memory, new CancellationToken(true)).ConfigureAwait(false)
        ).ConfigureAwait(false);
        exception.Should().Be(_otherTokenCanceledException);
    }

    [Fact]
    public async void TestReadAsyncSourceCancelledThrows()
    {
        var (sut,source) = SetupForRead();

        var firstResult = await sut.ReadAsync(_memory).ConfigureAwait(false);
        firstResult.Should().Be(_numBytesFirstRead);

        source.Cancel();
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await sut.ReadAsync(_memory).ConfigureAwait(false)
        ).ConfigureAwait(false);
        exception.Should().Be(_sourceTokenCanceledException);
    }

    #endregion

    #region Setup
    
    private (Stream,CancellationTokenSource) SetupForRead()
    {
        var source = new CancellationTokenSource();
        A.CallTo(() => _getNumBytes()).ReturnsNextFromSequence(_numBytesFirstRead,_numBytesSecondRead);
        A.CallTo(() => _stream.CanRead).Returns(true);
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.That.IsNotCanceled())).ReturnsLazily(() => _getNumBytes());
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.That.Matches(t => t == source.Token && t.CanBeCanceled && t.IsCancellationRequested))).Throws(_sourceTokenCanceledException);
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.That.Matches(t => t != source.Token && t.CanBeCanceled && t.IsCancellationRequested))).Throws(_otherTokenCanceledException);
        return (new CancellableStream(_stream,source.Token),source);
    }

    private (Stream,CancellationTokenSource) SetupForWrite()
    {
        var source = new CancellationTokenSource();
        A.CallTo(() => _stream.CanWrite).Returns(true);
        A.CallTo(() => _stream.WriteAsync(A<ReadOnlyMemory<byte>>.Ignored, A<CancellationToken>.That.IsNotCanceled())).Returns(ValueTask.CompletedTask);
        A.CallTo(() => _stream.WriteAsync(A<ReadOnlyMemory<byte>>.Ignored, A<CancellationToken>.That.Matches(t => t == source.Token && t.CanBeCanceled && t.IsCancellationRequested))).Throws(_sourceTokenCanceledException);
        A.CallTo(() => _stream.WriteAsync(A<ReadOnlyMemory<byte>>.Ignored, A<CancellationToken>.That.Matches(t => t != source.Token && t.CanBeCanceled && t.IsCancellationRequested))).Throws(_otherTokenCanceledException);
        return (new CancellableStream(_stream,source.Token),source);
    }

    #endregion
}