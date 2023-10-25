using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Models;

public class OwnCompanyUserEditableDetailsTests
{
    [Theory]
    [InlineData("postmaster@[IPv6:2001:0db8:85a3:0000:0000:8a2e:0370:7334]")]
    [InlineData("postmaster@[123.123.123.123]")]
    [InlineData("user-@example.org")]
    [InlineData("user%example.com@example.org")]
    [InlineData("mailhost!username@example.org")]
    [InlineData("\"john..doe\"@example.org")]
    [InlineData("\" \"@example.org")]
    [InlineData("example@s.example")]
    [InlineData("admin@example")]
    [InlineData("name/surname@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("long.email-address-with-hyphens@and.subdomains.example.com")]
    [InlineData("x@example.com")]
    [InlineData("very.common@example.com")]
    [InlineData("simple@example.com")]
    [InlineData("\"foo bar\" <foo@bar.org>")]
    [InlineData("foo bar <foo@bar.org>")]
    public void ValidateEmail_WithValidEmail_ReturnsExpected(string email)
    {
        // Arrange
        var sut = new OwnCompanyUserEditableDetails { FirstName = "fixed", Email = email, LastName = "fixed" };

        // Act
        var result = Validator.TryValidateObject(sut, new ValidationContext(sut, null, null), null, true);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc.example.com")]
    [InlineData("a@b@c@example.com")]
    [InlineData("\"foo bar\" <foo@bar@baz.org>")]
    public void ValidateEmail_WithInvalidEmail_ReturnsExpected(string email)
    {
        // Arrange
        var sut = new OwnCompanyUserEditableDetails { FirstName = "fixed", Email = email, LastName = "fixed" };

        // Act
        var result = Validator.TryValidateObject(sut, new ValidationContext(sut, null, null), null, true);

        // Assert
        result.Should().BeFalse();
    }
}
