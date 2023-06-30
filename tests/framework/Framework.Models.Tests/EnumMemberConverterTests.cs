using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests;

public class EnumMemberConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public void Read_WithDuplicateEnumMember_ThrowsUnexpectedCondition()
    {
        // Arrange
        const string json = "{\"enumValue\":\"This value is duplicated\"}";
        var Act = () => JsonSerializer.Deserialize<DuplicateEnumMemberTestClass>(json, Options);

        // Act
        var ex = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be($"There should only be one EnumMember of {typeof(TestEnumWithSameEnumMembers)} configured for value 'This value is duplicated'");
    }

    [Fact]
    public void Read_WithValidJson_DoesExpected()
    {
        // Arrange
        const string json = "{\"testString\":\"just the string value\",\"enumValue\":\"This is a test\"}";

        // Act
        var model = JsonSerializer.Deserialize<EnumMemberTestClass>(json, Options);

        // Assert
        model.Should().NotBeNull();
        model!.EnumValue.Should().Be(TestEnum.Test);
        model.TestString.Should().Be("just the string value");
    }

    [Fact]
    public void Read_WithOneEnumMember_DoesExpected()
    {
        // Arrange
        const string json = "{\"enumValue\":\"This is a test\"}";

        // Act
        var model = JsonSerializer.Deserialize<EnumWithOnlyOneEnumMemberTestClass>(json, Options);

        // Assert
        model.Should().NotBeNull();
        model!.EnumValue.Should().Be(EnumWithOnlyOneEnumMember.Test);
    }

    [Fact]
    public void Write_WithValid_DoesExpected()
    {
        // Arrange
        var test = new EnumMemberTestClass
        {
            TestString = "just the string value",
            EnumValue = TestEnum.Test
        };

        // Act
        var json = JsonSerializer.Serialize(test, Options);

        // Assert
        json.Should().Be("{\"testString\":\"just the string value\",\"enumValue\":\"This is a test\"}");
    }

    #region Setup

    internal enum TestEnum
    {
        [EnumMember(Value = "This is a test")]
        Test = 1,

        [EnumMember(Value = "This is a value")]
        Value = 2
    }

    internal enum TestEnumWithSameEnumMembers
    {
        [EnumMember(Value = "This value is duplicated")]
        Test = 1,

        [EnumMember(Value = "This value is duplicated")]
        Value = 2
    }

    internal enum EnumWithOnlyOneEnumMember
    {
        [EnumMember(Value = "This is a test")]
        Test = 1,

        Value = 2
    }

    internal class DuplicateEnumMemberTestClass
    {
        [JsonConverter(typeof(EnumMemberConverter<TestEnumWithSameEnumMembers>))]
        public TestEnumWithSameEnumMembers EnumValue { get; set; }
    }

    internal class EnumMemberTestClass
    {
        public string TestString { get; set; } = null!;

        [JsonConverter(typeof(EnumMemberConverter<TestEnum>))]
        public TestEnum EnumValue { get; set; }
    }

    internal class EnumWithOnlyOneEnumMemberTestClass
    {
        public string TestString { get; set; } = null!;

        [JsonConverter(typeof(EnumMemberConverter<EnumWithOnlyOneEnumMember>))]
        public EnumWithOnlyOneEnumMember EnumValue { get; set; }
    }

    #endregion
}
