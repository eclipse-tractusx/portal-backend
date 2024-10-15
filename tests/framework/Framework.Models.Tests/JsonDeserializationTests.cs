/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests;

public class JsonDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public void Serialize_WithSettings_ReturnsExpected()
    {
        // Arrange
        var test = new TestClass
        {
            TestDict = new Dictionary<string, string>
            {
                { "TestUpper", "testValue" },
                { "testSnakeCase", "test" },
                { "testalllower", "testValue" },
                { "TESTALLUPPER", "testValue" }
            }
        };

        // Act
        var serialized = JsonSerializer.Serialize(test, JsonOptions);

        // Assert
        serialized.Should().Be("""{"testDict":{"TestUpper":"testValue","testSnakeCase":"test","testalllower":"testValue","TESTALLUPPER":"testValue"}}""");
    }

    [Fact]
    public void Deserialize_WithSettings_ReturnsExpected()
    {
        const string Json = """{"testDict":{"TestUpper":"testValue","testSnakeCase":"test","testalllower":"test","TESTALLUPPER":"test"}}""";

        // Act
        var serialized = JsonSerializer.Deserialize<TestClass>(Json, JsonOptions);

        // Assert
        serialized.Should().NotBeNull();
        serialized!.TestDict.Should().ContainKeys("TestUpper", "testSnakeCase", "testalllower", "TESTALLUPPER");
    }

    internal class TestClass
    {
        public IDictionary<string, string> TestDict { get; set; } = null!;
    }
}
