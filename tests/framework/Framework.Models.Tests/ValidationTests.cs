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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests;

public class ValidationTests
{
    private readonly IFixture _fixture;
    public ValidationTests()
    {
        _fixture = new Fixture();
    }

    #region DistinctValuesValidation

    public class DistinctValuesTypedPropery
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class DistinctValuesTestSettings
    {
        [DistinctValues]
        public IEnumerable<string> StringProperty { get; set; } = null!;

        [DistinctValues("x => x.Key")]
        public IEnumerable<DistinctValuesTypedPropery> TypedProperty { get; set; } = null!;
    }

    public class InvalidDistinctValuesTestSettings
    {
        [DistinctValues("x => x.Foo")]
        public IEnumerable<DistinctValuesTypedPropery> InvalidProperty { get; set; } = null!;
    }

    public class WrappedDistinctValuesTestSettings
    {
        public DistinctValuesTestSettings TestSettings { get; set; } = null!;
    }

    [Fact]
    public void DistinctValuesValidation_Distinct_ReturnsExpected()
    {
        // Arrange
        const string configuration = @"
        {
            ""StringProperty"": [
                ""foo"",
                ""bar"",
                ""baz""
            ],
            ""TypedProperty"": [
                {
                    ""Key"": ""foo"",
                    ""Value"": ""value1""
                },
                {
                    ""Key"": ""bar"",
                    ""Value"": ""value2""
                },
                {
                    ""Key"": ""baz"",
                    ""Value"": ""value3""
                }
            ]
        }";
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration))).Build();
        var settings = config.Get<DistinctValuesTestSettings>();
        var name = _fixture.Create<string>();

        var sut = new DistinctValuesValidation<DistinctValuesTestSettings>(name, config);

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            r.Succeeded &&
            !r.Failed &&
            r.FailureMessage == null
        );
    }

    [Fact]
    public void DistinctValuesValidation_Missing_ReturnsExpected()
    {
        // Arrange
        const string configuration = @"
        {
            ""StringProperty"": [],
            ""TypedProperty"": []
        }";
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration))).Build();
        var settings = new DistinctValuesTestSettings();
        var name = _fixture.Create<string>();

        var sut = new DistinctValuesValidation<DistinctValuesTestSettings>(name, config);

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            r.Succeeded &&
            !r.Failed &&
            r.FailureMessage == null
        );
    }

    [Fact]
    public void DistinctValuesValidation_Duplicates_ReturnsExpected()
    {
        // Arrange
        const string configuration = @"
        {
            ""StringProperty"": [
                ""foo"",
                ""bar"",
                ""foo""
            ],
            ""TypedProperty"": [
                {
                    ""Key"": ""foo"",
                    ""Value"": ""value1""
                },
                {
                    ""Key"": ""bar"",
                    ""Value"": ""value2""
                },
                {
                    ""Key"": ""foo"",
                    ""Value"": ""value3""
                }
            ]
        }";
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration))).Build();
        var settings = config.Get<DistinctValuesTestSettings>();
        var name = _fixture.Create<string>();

        var sut = new DistinctValuesValidation<DistinctValuesTestSettings>(name, config);

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            !r.Succeeded &&
            r.Failed &&
            r.FailureMessage == "DataAnnotation validation failed for members: 'StringProperty' with the error: 'foo are duplicate values for StringProperty.'.; DataAnnotation validation failed for members: 'TypedProperty' with the error: 'Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests.ValidationTests+DistinctValuesTypedPropery are duplicate values for TypedProperty.'."
        );
    }

    [Fact]
    public void DistinctValuesValidation_InvalidProperty_ThrowsExpected()
    {
        // Arrange
        const string configuration = @"
        {
            ""InvalidProperty"": [
                {
                    ""Key"": ""foo"",
                    ""Value"": ""value1""
                },
                {
                    ""Key"": ""bar"",
                    ""Value"": ""value2""
                },
                {
                    ""Key"": ""foo"",
                    ""Value"": ""value3""
                }
            ]
        }";
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration))).Build();
        var settings = config.Get<InvalidDistinctValuesTestSettings>();
        var name = _fixture.Create<string>();

        var sut = new DistinctValuesValidation<InvalidDistinctValuesTestSettings>(name, config);

        var Act = () => sut.Validate(name, settings);

        // Act
        var result = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        result.Should().NotBeNull().And.Match<UnexpectedConditionException>(r =>
            r.Message == "invalid selector x => x.Foo for type Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests.ValidationTests+DistinctValuesTypedPropery");
    }

    [Fact]
    public void DistinctValuesValidation_EmptyInvalidProperty_ThrowsExpected()
    {
        // Arrange
        const string configuration = @"
        {
            ""InvalidProperty"": []
        }";
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration))).Build();
        var settings = new InvalidDistinctValuesTestSettings();
        var name = _fixture.Create<string>();

        var sut = new DistinctValuesValidation<InvalidDistinctValuesTestSettings>(name, config);

        var Act = () => sut.Validate(name, settings);

        // Act
        var result = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        result.Should().NotBeNull().And.Match<UnexpectedConditionException>(r =>
            r.Message == "invalid selector x => x.Foo for type Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests.ValidationTests+DistinctValuesTypedPropery");
    }

    [Fact]
    public void DistinctValuesValidation_WrappedDuplicates_ReturnsExpected()
    {
        // Arrange
        const string configuration = @"
        {
            ""TestSettings"": {
                ""StringProperty"": [
                    ""foo"",
                    ""bar"",
                    ""foo""
                ],
                ""TypedProperty"": [
                    {
                        ""Key"": ""foo"",
                        ""Value"": ""value1""
                    },
                    {
                        ""Key"": ""bar"",
                        ""Value"": ""value2""
                    },
                    {
                        ""Key"": ""foo"",
                        ""Value"": ""value3""
                    }
                ]   
            }
        }";
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration))).Build();
        var settings = config.Get<WrappedDistinctValuesTestSettings>();
        var name = _fixture.Create<string>();

        var sut = new DistinctValuesValidation<WrappedDistinctValuesTestSettings>(name, config);

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            !r.Succeeded &&
            r.Failed &&
            r.FailureMessage == "DataAnnotation validation failed for members: 'StringProperty' with the error: 'foo are duplicate values for StringProperty.'.; DataAnnotation validation failed for members: 'TypedProperty' with the error: 'Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests.ValidationTests+DistinctValuesTypedPropery are duplicate values for TypedProperty.'."
        );
    }

    #endregion

    #region EnumEnumerableValidation

    public enum TestEnum
    {
        Foo = 1,
        Bar = 2,
        Baz = 3
    }

    public class EnumEnumerableTestSettings
    {
        [EnumEnumeration]
        public IEnumerable<TestEnum> EnumProperty { get; set; } = null!;
    }

    public class InvalidEnumEnumerableTestSettings
    {
        [EnumEnumeration]
        public IEnumerable<string> InvalidProperty { get; set; } = null!;
    }

    public class WrapperInvalidEnumEnumerableTestSettings
    {
        public EnumEnumerableTestSettings EnumValue { get; set; } = null!;
    }

    [Fact]
    public void EnumEnumerableValidation_Valid_ReturnsExpected()
    {
        // Arrange
        var appSettings = @"
        {
            ""EnumProperty"": [
                ""Foo"",
                ""Bar""
            ]
        }";

        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings))).Build();
        var name = _fixture.Create<string>();

        var sut = new EnumEnumerableValidation<EnumEnumerableTestSettings>(name, config);

        var settings = config.Get<EnumEnumerableTestSettings>();

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            r.Succeeded &&
            !r.Failed &&
            r.FailureMessage == null
        );
    }

    [Fact]
    public void EnumEnumerableValidation_Extra_ReturnsExpected()
    {
        // Arrange
        var appSettings = @"
        {
            ""EnumProperty"": [
                ""Foo"",
                ""Bar"",
                ""Extra""
            ]
        }";

        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings))).Build();
        var name = _fixture.Create<string>();

        var sut = new EnumEnumerableValidation<EnumEnumerableTestSettings>(name, config);

        var settings = config.Get<EnumEnumerableTestSettings>();

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            !r.Succeeded &&
            r.Failed &&
            r.FailureMessage == "DataAnnotation validation failed for members: 'EnumProperty' with the error: 'Extra is not a valid value for Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests.ValidationTests+TestEnum in section EnumProperty. Valid values are: Foo, Bar, Baz'."
        );
    }

    [Fact]
    public void EnumEnumerableValidation_Invalid_ThrowsExpected()
    {
        // Arrange
        var appSettings = @"
        {
            ""InvalidProperty"": [
                ""Foo"",
                ""Bar""
            ]
        }";

        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings))).Build();
        var name = _fixture.Create<string>();

        var sut = new EnumEnumerableValidation<InvalidEnumEnumerableTestSettings>(name, config);

        var settings = config.Get<InvalidEnumEnumerableTestSettings>();

        var Act = () => sut.Validate(name, settings);

        // Act
        var result = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        result.Should().NotBeNull().And.Match<UnexpectedConditionException>(r =>
            r.Message == "InvalidProperty must be of type IEnumerable<Enum> but is IEnumerable<System.String>"
        );
    }

    [Fact]
    public void EnumEnumerableValidation_WithWrappedInvalidProperty_ThrowsExpected()
    {
        // Arrange
        const string appSettings = @"
        {
            ""EnumValue"": {
                ""EnumProperty"": [
                    ""Foo"",
                    ""Bar"",
                    ""Extra""
                ]
            }
        }";

        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings))).Build();
        var name = _fixture.Create<string>();

        var sut = new EnumEnumerableValidation<WrapperInvalidEnumEnumerableTestSettings>(name, config);

        var settings = config.Get<WrapperInvalidEnumEnumerableTestSettings>();

        // Act
        var result = sut.Validate(name, settings);

        // Assert
        result.Should().NotBeNull().And.Match<ValidateOptionsResult>(r =>
            !r.Skipped &&
            !r.Succeeded &&
            r.Failed &&
            r.FailureMessage == "DataAnnotation validation failed for members: 'EnumProperty' with the error: 'Extra is not a valid value for Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests.ValidationTests+TestEnum in section EnumValue:EnumProperty. Valid values are: Foo, Bar, Baz'."
        );
    }

    #endregion

    #region ValidateEnumValuesAttribute

    [Theory]
    [InlineData(new[] { TestEnum.Foo, TestEnum.Bar }, true)]
    [InlineData(new[] { TestEnum.Foo, (TestEnum)3 }, true)]
    [InlineData(new[] { TestEnum.Foo, (TestEnum)0 }, false)]
    [InlineData(new TestEnum[] { }, true)]
    public void ValidateEnumValuesAttribute_ReturnsExpected(IEnumerable<TestEnum> values, bool expected)
    {
        // Arrange
        var sut = new ValidateEnumValuesAttribute();

        // Act
        var result = sut.IsValid(values);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ValidateEnumValueAttribute

    [Theory]
    [InlineData(TestEnum.Foo, true)]
    [InlineData((TestEnum)2, true)]
    [InlineData((TestEnum)0, false)]
    public void ValidateEnumValueAttribute_ReturnsExpected(TestEnum value, bool expected)
    {
        // Arrange
        var sut = new ValidateEnumValueAttribute();

        // Act
        var result = sut.IsValid(value);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
