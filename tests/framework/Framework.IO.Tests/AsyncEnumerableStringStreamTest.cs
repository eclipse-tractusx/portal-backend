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
using FluentAssertions;
using Xunit;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO.Tests;

public class AsyncEnumerableStringStreamTest
{
    private readonly IFixture _fixture;
    private readonly IEnumerable<string> _data;
    private readonly Encoding _encoding;

    public AsyncEnumerableStringStreamTest()
    {
        _fixture = new Fixture();

        _data = _fixture.CreateMany<string>(20);
        _encoding = _fixture.Create<Encoding>();
    }

    [Fact]
    public void TestMetadata()
    {
        var sut = new AsyncEnumerableStringStream(_data.ToAsyncEnumerable(),_encoding);
        sut.CanRead.Should().BeTrue();
        sut.CanSeek.Should().BeFalse();
        sut.CanTimeout.Should().BeFalse();
        sut.CanWrite.Should().BeFalse();
        Assert.Throws<NotSupportedException>(() => sut.Length);
        Assert.Throws<NotSupportedException>(() => sut.Position = _fixture.Create<long>());
        Assert.Throws<NotSupportedException>(() => sut.Position);
        Assert.Throws<NotSupportedException>(() => sut.Seek(_fixture.Create<long>(),_fixture.Create<SeekOrigin>()));
        Assert.Throws<NotSupportedException>(() => sut.Flush());
        Assert.Throws<NotSupportedException>(() => sut.Read(_fixture.Create<byte[]>(),_fixture.Create<int>(),_fixture.Create<int>()));
        Assert.Throws<NotSupportedException>(() => sut.Write(_fixture.Create<byte[]>(),_fixture.Create<int>(),_fixture.Create<int>())); 
        Assert.Throws<NotSupportedException>(() => sut.SetLength(_fixture.Create<long>())); 
    }

    [Fact]
    public async void TestAsyncCopyToSuccess()
    {
        using var expected = GetExpected();

        var sut = new AsyncEnumerableStringStream(_data.ToAsyncEnumerable(),_encoding);

        using var result = new MemoryStream();
        await sut.CopyToAsync(result).ConfigureAwait(false);

        result.ToArray().SequenceEqual(expected.ToArray()).Should().BeTrue();
    }

    private MemoryStream GetExpected()
    {
        var praeamble = _encoding.GetPreamble();
        var dataBytes = _encoding.GetBytes(string.Join(Environment.NewLine,_data)+Environment.NewLine);
        var ms = new MemoryStream(praeamble.Length + dataBytes.Length);
        ms.Write(praeamble,0,praeamble.Length);
        ms.Write(dataBytes,0,dataBytes.Length);
        return ms;
    }
}
