/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using FakeItEasy;
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Xunit;
using System.Text;

namespace Org.CatenaX.Ng.Portal.Backend.Framework.IO.Tests;

public class CsvParserTest
{
    private readonly Stream _stream;
    private readonly Action<string> _validateHeaderLine;
    private readonly Func<string,FakeLineType> _parseLine;
    private readonly FakeLineType _parseLineResult;
    private readonly Func<IAsyncEnumerable<FakeLineType>,IAsyncEnumerable<(bool Processed, Exception? Error)>> _processLines;
    private readonly CancellationToken _cancellationToken;
    public class FakeLineType {};

    public CsvParserTest()
    {
        _stream = A.Fake<Stream>();
        _validateHeaderLine = A.Fake<Action<string>>();
        _parseLine = A.Fake<Func<string,FakeLineType>>();
        _parseLineResult = A.Fake<FakeLineType>();
        _processLines = A.Fake<Func<IAsyncEnumerable<FakeLineType>,IAsyncEnumerable<(bool Processed, Exception? Error)>>>();
        _cancellationToken = new CancellationToken();

        SetupFakes();
    }

    [Fact]
    public async Task TestProcessCsvAsyncEmptyFileThrows()
    {
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.Ignored)).Returns(0);
        
        async Task Act () => await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("uploaded file contains no lines (Parameter 'document')");
        A.CallTo(() => _validateHeaderLine(A<string>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestProcessCsvAsyncNoValidateHaderLineEmptyFile()
    {
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.Ignored)).Returns(0);
        
        await CsvParser.ProcessCsvAsync(
            _stream,
            null,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        A.CallTo(() => _parseLine(A<string>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TestProcessCsvAsyncHeaderNoData()
    {
        using var data = SetupStream("header line\n");

        await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        A.CallTo(() => _validateHeaderLine(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _parseLine(A<string>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TestProcessCsvAsyncHeaderWithData()
    {
        using var data = SetupStream("header line\nfirst line\nsecond line\nthird line\nforth line\n");

        await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        A.CallTo(() => _validateHeaderLine(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _parseLine(A<string>.Ignored)).MustHaveHappened(4, Times.Exactly);
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TestProcessCsvAsyncHeaderThrowsWithData()
    {
        A.CallTo(() => _validateHeaderLine(A<string>.Ignored)).Throws(new ArgumentException("invalid header"));
        using var data = SetupStream("header line\nfirst line\nsecond line\nthird line\nforth line\n");

        async Task Act () => await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);
        exception.Message.Should().Be("invalid header");
        A.CallTo(() => _validateHeaderLine(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _parseLine(A<string>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestProcessCsvAsyncHeaderWithDataParseLineThrows()
    {
        using var data = SetupStream("header line\nfirst line\nsecond line\nthird line\nforth line\n");
        A.CallTo(() => _parseLine(A<string>.That.Matches(s => s == "third line"))).Throws(new ArgumentException("invalid line"));

        var result = await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        A.CallTo(() => _validateHeaderLine(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _parseLine(A<string>.Ignored)).MustHaveHappened(4, Times.Exactly);
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustHaveHappenedOnceExactly();
        result.Processed.Should().Be(3);
        result.Lines.Should().Be(4);
        result.Errors.Count().Should().Be(1);
        var error = result.Errors.First();
        error.Line.Should().Be(3);
        error.Error.Should().BeOfType(typeof(ArgumentException));
        error.Error.Message.Should().Be("invalid line");
    }

    [Fact]
    public async Task TestProcessCsvAsyncHeaderWithDataProcessLinesError()
    {
        using var data = SetupStream("header line\nfirst line\nsecond line\nthird line\nforth line\n");

        var processLineResults = new [] {
            (Processed: true, Error: (Exception?)null),
            (Processed: true, Error: (Exception?)null),
            (Processed: false, Error: new ArgumentException("error processing")),
            (Processed: true, Error: (Exception?)null)
        }.ToAsyncEnumerable();

        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).Returns(processLineResults);

        var result = await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustHaveHappenedOnceExactly();
        result.Processed.Should().Be(3);
        result.Lines.Should().Be(4);
        result.Errors.Count().Should().Be(1);
        var error = result.Errors.First();
        error.Line.Should().Be(3);
        error.Error.Should().BeOfType(typeof(ArgumentException));
        error.Error.Message.Should().Be("error processing");
    }

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _stream.CanRead).Returns(true);
        A.CallTo(() => _parseLine(A<string>.Ignored)).Returns(_parseLineResult);
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).ReturnsLazily<IAsyncEnumerable<(bool Processed, Exception? Error)>,IAsyncEnumerable<FakeLineType>>(lines => FakeReadLinesSuccess(lines));
    }

    private MemoryStream SetupStream(string stringdata)
    {
        var data = new MemoryStream(Encoding.UTF8.GetBytes(stringdata));
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.Ignored)).ReturnsLazily<ValueTask<int>,Memory<byte>,CancellationToken>((Memory<byte> memory, CancellationToken cancellationToken) => data.ReadAsync(memory, cancellationToken));
        return data;
    }

    private static async IAsyncEnumerable<(bool Processed, Exception? Error)> FakeReadLinesSuccess(IAsyncEnumerable<FakeLineType> lines)
    {
        await foreach(var line in lines)
        {
            yield return (true,null);
        }
    }

    #endregion
}