/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Xunit;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO.Tests;

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

    #region Validation

    [Fact]
    public void ValidateContentTypeTextCSVSuccess()
    {
        CsvParser.ValidateContentTypeTextCSV("text/csv");
    }

    [Fact]
    public void ValidateContentTypeTextCSVThrows()
    {
        var exception = Assert.Throws<UnsupportedMediaTypeException>(() => CsvParser.ValidateContentTypeTextCSV("other"));
        exception.Message.Should().Be("Only contentType text/csv files are allowed.");
    }

    [Fact]
    public void ValidateCsvHeadersSuccess()
    {
        CsvParser.ValidateCsvHeaders("first,second,third", new [] { "first", "second", "third"});
    }

    [Fact]
    public void ValidateCsvHeadersInvalidHeaderThrows()
    {
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.ValidateCsvHeaders("first,other,third", new [] { "first", "second", "third"}));
        exception.Message.Should().Be($"invalid format: expected 'second', got 'other' (Parameter 'document')");
    }

    [Fact]
    public void ValidateCsvHeadersLessHeadersThrows()
    {
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.ValidateCsvHeaders("first,second", new [] { "first", "second", "third"}));
        exception.Message.Should().Be($"invalid format: expected 'third', got '' (Parameter 'document')");
    }

    [Fact]
    public void ValidateCsvHeadersMoreHeadersThrows()
    {
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.ValidateCsvHeaders("first,second,third,forth", new [] { "first", "second", "third"}));
        exception.Message.Should().Be($"unexpected header 'forth' (Parameter 'document')");
    }

    [Fact]
    public void NextStringItemIsNotNullSuccess()
    {
        var items = A.Fake<IEnumerator<string>>();
        A.CallTo(() => items.MoveNext()).Returns(true);
        A.CallTo(() => items.Current).Returns("item");
        var result = CsvParser.NextStringItemIsNotNull(items, "itemName");
        result.Should().Be("item");
    }

    [Fact]
    public void NextStringItemIsNotNullNoItemThrows()
    {
        var items = A.Fake<IEnumerator<string>>();
        A.CallTo(() => items.MoveNext()).Returns(false);
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.NextStringItemIsNotNull(items, "itemName"));
        exception.Message.Should().Be("value for itemName type string expected (Parameter 'document')");
    }

    [Fact]
    public void NextStringItemIsNotNullOrWhiteSpaceSuccess()
    {
        var items = A.Fake<IEnumerator<string>>();
        A.CallTo(() => items.MoveNext()).Returns(true);
        A.CallTo(() => items.Current).Returns("item");
        var result = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, "itemName");
        result.Should().Be("item");
    }

    [Fact]
    public void NextStringItemIsNotNullOrWhiteSpaceNoItemThrows()
    {
        var items = A.Fake<IEnumerator<string>>();
        A.CallTo(() => items.MoveNext()).Returns(false);
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, "itemName"));
        exception.Message.Should().Be("value for itemName type string expected (Parameter 'document')");
    }

    [Fact]
    public void NextStringItemIsNotNullOrWhiteSpaceEmptyItemThrows()
    {
        var items = A.Fake<IEnumerator<string>>();
        A.CallTo(() => items.MoveNext()).Returns(true);
        A.CallTo(() => items.Current).Returns(string.Empty);
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, "itemName"));
        exception.Message.Should().Be("value for itemName type string expected (Parameter 'document')");
    }

    [Fact]
    public void NextStringItemIsNotNullOrWhiteSpaceWhitespaceItemThrows()
    {
        var items = A.Fake<IEnumerator<string>>();
        A.CallTo(() => items.MoveNext()).Returns(true);
        A.CallTo(() => items.Current).Returns("   ");
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, "itemName"));
        exception.Message.Should().Be("value for itemName type string expected (Parameter 'document')");
    }

    [Fact]
    public void TrailingStringItemsNotNullOrWhiteSpaceSuccess()
    {
        var items = new List<string> { "item1", "item2", "item3" }.GetEnumerator() as IEnumerator<string>;
        var result = CsvParser.TrailingStringItemsNotNullOrWhiteSpace(items, "itemName").ToList();
        result.Should().BeEquivalentTo(new [] {"item1", "item2", "item3"});
    }

    [Fact]
    public void TrailingStringItemsNotNullOrWhiteSpaceEmptyItemThrows()
    {
        var items = new List<string> { "item1", "", "item3" }.GetEnumerator() as IEnumerator<string>;
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.TrailingStringItemsNotNullOrWhiteSpace(items, "itemName").ToList());
        exception.Message.Should().Be("value for itemName type string expected (Parameter 'document')");
    }

    [Fact]
    public void TrailingStringItemsNotNullOrWhiteSpaceWhiteSpaceItemThrows()
    {
        var items = new List<string> { "item1", "   ", "item3" }.GetEnumerator() as IEnumerator<string>;
        var exception = Assert.Throws<ControllerArgumentException>(() => CsvParser.TrailingStringItemsNotNullOrWhiteSpace(items, "itemName").ToList());
        exception.Message.Should().Be("value for itemName type string expected (Parameter 'document')");
    }

    #endregion

    #region ProcessCsvAsync

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

        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).ReturnsLazily<IAsyncEnumerable<(bool Processed, Exception? Error)>,IAsyncEnumerable<FakeLineType>>(lines => ProcessLinesError(lines));

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
        error.Error.Should().BeOfType(typeof(ControllerArgumentException));
        error.Error.Message.Should().Be("error processing");
    }

    [Fact]
    public async Task TestProcessCsvAsyncHeaderWithDataProcessLinesThrows()
    {
        using var data = SetupStream("header line\nfirst line\nsecond line\nthird line\nforth line\n");

        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).ReturnsLazily<IAsyncEnumerable<(bool Processed, Exception? Error)>,IAsyncEnumerable<FakeLineType>>(lines => ProcessLinesThrows(lines));

        var result = await CsvParser.ProcessCsvAsync(
            _stream,
            _validateHeaderLine,
            _parseLine,
            _processLines,
            _cancellationToken).ConfigureAwait(false);

        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).MustHaveHappenedOnceExactly();
        result.Processed.Should().Be(2);
        result.Lines.Should().Be(3);
        result.Errors.Count().Should().Be(1);
        var error = result.Errors.First();
        error.Line.Should().Be(3);
        error.Error.Should().BeOfType(typeof(UnexpectedConditionException));
        error.Error.Message.Should().Be("unexpected error");
    }

    #endregion

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _stream.CanRead).Returns(true);
        A.CallTo(() => _parseLine(A<string>.Ignored)).Returns(_parseLineResult);
        A.CallTo(() => _processLines(A<IAsyncEnumerable<FakeLineType>>.Ignored)).ReturnsLazily<IAsyncEnumerable<(bool Processed, Exception? Error)>,IAsyncEnumerable<FakeLineType>>(lines => ProcessLinesSuccess(lines));
    }

    private MemoryStream SetupStream(string stringdata)
    {
        var data = new MemoryStream(Encoding.UTF8.GetBytes(stringdata));
        A.CallTo(() => _stream.ReadAsync(A<Memory<byte>>.Ignored, A<CancellationToken>.Ignored)).ReturnsLazily<ValueTask<int>,Memory<byte>,CancellationToken>((Memory<byte> memory, CancellationToken cancellationToken) => data.ReadAsync(memory, cancellationToken));
        return data;
    }

    private static async IAsyncEnumerable<(bool Processed, Exception? Error)> ProcessLinesSuccess(IAsyncEnumerable<FakeLineType> lines)
    {
        await foreach(var line in lines)
        {
            yield return (true,null);
        }
    }

    private async static IAsyncEnumerable<(bool Processed, Exception? Error)> ProcessLinesError(IAsyncEnumerable<FakeLineType> lines)
    {
        int numLine = 0;
        await foreach(var line in lines)
        {
            numLine++;
            yield return numLine == 3
                ? (false, new ControllerArgumentException("error processing"))
                : (true,null);
        }
    }

    private async static IAsyncEnumerable<(bool Processed, Exception? Error)> ProcessLinesThrows(IAsyncEnumerable<FakeLineType> lines)
    {
        int numLine = 0;
        await foreach(var line in lines)
        {
            numLine++;
            if (numLine == 3)
            {
                throw new UnexpectedConditionException("unexpected error");
            }
            yield return (true,null);
        }
    }

    #endregion
}