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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class ContentTypeMapperExtensionTests
{
    private readonly IFixture _fixture;

    public ContentTypeMapperExtensionTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    #region MapToImageContentType

    [Theory]
    [InlineData("filename.jpg", "image/jpeg")]
    [InlineData("filename.jpeg", "image/jpeg")]
    [InlineData("filename.png", "image/png")]
    [InlineData("filename.gif", "image/gif")]
    [InlineData("filename.svg", "image/svg+xml")]
    [InlineData("filename.tif", "image/tiff")]
    [InlineData("filename.tiff", "image/tiff")]
    [InlineData("filename.JPG", "image/jpeg")]
    [InlineData("filename.JPEG", "image/jpeg")]
    [InlineData("filename.PNG", "image/png")]
    [InlineData("filename.GIF", "image/gif")]
    [InlineData("filename.SVG", "image/svg+xml")]
    [InlineData("filename.TIF", "image/tiff")]
    [InlineData("filename.TIFF", "image/tiff")]

    public void MapToImageContentType_ExpectedResult(string filename, string contentType)
    {
        var result = filename.MapToImageContentType();
        result.Should().Be(contentType);
    }

    [Theory]
    [InlineData("deadbeaf")]
    [InlineData("deadbeaf.pdf")]
    public void MapToImageContentType_Throws(string filename)
    {
        var Act = () => filename.MapToImageContentType();
        var result = Assert.Throws<UnsupportedMediaTypeException>(Act);
    }

    #endregion
}
