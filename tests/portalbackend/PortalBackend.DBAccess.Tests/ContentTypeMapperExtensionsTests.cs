using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Reflection.Metadata;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ContentTypeMapperExtensionsTests
{
	[Theory]
	[InlineData(MediaTypeId.GIF, "image/gif")]
	[InlineData(MediaTypeId.PDF, "application/pdf")]
	[InlineData(MediaTypeId.PNG, "image/png")]
	[InlineData(MediaTypeId.SVG, "image/svg+xml")]
	[InlineData(MediaTypeId.JSON, "application/json")]
	[InlineData(MediaTypeId.JPEG, "image/jpeg")]
	[InlineData(MediaTypeId.TIFF, "image/tiff")]
	[InlineData(MediaTypeId.PEM, "application/x-pem-file")]
	[InlineData(MediaTypeId.CA_CERT, "application/x-x509-ca-cert")]
	[InlineData(MediaTypeId.PKX_CER, "application/pkix-cert")]
	[InlineData(MediaTypeId.OCTET, "application/octet-stream")]
	public void MapToMediaType_WithValid_ReturnsExpected(MediaTypeId mediaTypeId, string result)
	{
		var mediaType = mediaTypeId.MapToMediaType();
		mediaType.Should().Be(result);
	}

	[Fact]
	public void MapToMediaType_WithInvalid_ThrowsConflictException()
	{
		void Act() => ((MediaTypeId)666).MapToMediaType();

		var ex = Assert.Throws<ConflictException>((Action)Act);
		ex.Message.Should().Be($"document mediatype 666 is not supported");
	}

	[Theory]
	[InlineData(MediaTypeId.GIF, "image/gif")]
	[InlineData(MediaTypeId.PDF, "application/pdf")]
	[InlineData(MediaTypeId.PNG, "image/png")]
	[InlineData(MediaTypeId.SVG, "image/svg+xml")]
	[InlineData(MediaTypeId.JSON, "application/json")]
	[InlineData(MediaTypeId.JPEG, "image/jpeg")]
	[InlineData(MediaTypeId.TIFF, "image/tiff")]
	[InlineData(MediaTypeId.PEM, "application/x-pem-file")]
	[InlineData(MediaTypeId.CA_CERT, "application/x-x509-ca-cert")]
	[InlineData(MediaTypeId.PKX_CER, "application/pkix-cert")]
	[InlineData(MediaTypeId.OCTET, "application/octet-stream")]
	public void ParseMediaTypeId_WithValid_ReturnsExpected(MediaTypeId expectedResult, string mediaType)
	{
		var result = mediaType.ParseMediaTypeId();
		result.Should().Be(expectedResult);
	}

	[Fact]
	public void ParseMediaTypeId_WithInvalid_ThrowsUnsupportedMediaTypeException()
	{
		void Act() => "just a test".ParseMediaTypeId();

		var ex = Assert.Throws<UnsupportedMediaTypeException>((Action)Act);
		ex.Message.Should().Be($"mediaType 'just a test' is not supported");
	}
}
