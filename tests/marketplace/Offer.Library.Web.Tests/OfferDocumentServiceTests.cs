/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

namespace Offer.Library.Web.Tests;

public class OfferDocumentServiceTests
{
    private static readonly Guid CompanyUserId = Guid.NewGuid();
    private static readonly Guid CompanyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _validAppId = Guid.NewGuid();
    private readonly IIdentityData _identity;

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly OfferDocumentService _sut;
    private readonly IIdentityService _identityService;

    public OfferDocumentServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(CompanyUserId);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(CompanyUserCompanyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        SetupCreateDocument();

        _sut = new OfferDocumentService(_portalRepositories, _identityService);
    }

    #region UploadDocument

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var documentId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var documents = new List<Document>();
        var offerAssignedDocuments = new List<OfferAssignedDocument>();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            });
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid docId) =>
            {
                var offerAssignedDocument = new OfferAssignedDocument(offerId, docId);
                offerAssignedDocuments.Add(offerAssignedDocument);
            });
        var existingOffer = _fixture.Create<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer>();
        existingOffer.DateLastChanged = DateTimeOffset.UtcNow;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(_validAppId, A<Action<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer>>._, A<Action<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer>?>._))
            .Invokes((Guid _, Action<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer> setOptionalParameters, Action<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        // Act
        await _sut.UploadDocumentAsync(_validAppId, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(_validAppId, A<Action<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer>>._, A<Action<Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities.Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
        offerAssignedDocuments.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_InValidData_ThrowsNotFoundException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, _identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns(((bool, bool, bool))default);

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} {id} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_EmptyId_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(Guid.Empty, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} id should not be null");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_EmptyFileName_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"File name should not be null");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_contentType_ThrowsUnsupportedMediaTypeException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "TestFile.txt", "image/svg+xml");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Document type {documentTypeId}, mediaType 'image/svg+xml' is not supported. File with contentType :{string.Join(",", uploadDocumentTypeIdSettings.Where(x => x.DocumentTypeId == documentTypeId).Select(x => x.MediaTypes).First())} are allowed.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_contentType_ThrowsUnsupportedMediaTypeException1(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "TestFile.txt", "foo/bar");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Document type {documentTypeId}, mediaType 'foo/bar' is not supported. File with contentType :{string.Join(",", uploadDocumentTypeIdSettings.Where(x => x.DocumentTypeId == documentTypeId).Select(x => x.MediaTypes).First())} are allowed.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.SELF_DESCRIPTION, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.APP_TECHNICAL_INFORMATION, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_documentType_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"documentType must be either: {string.Join(",", uploadDocumentTypeIdSettings.Select(x => x.DocumentTypeId))}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, OfferStatusId.CREATED)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, OfferStatusId.CREATED)]
    public async Task UploadDocumentAsync_isStatusCreated_ThrowsConflictException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferStatusId offerStatusId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.CompanyId).Returns(companyId);
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP
            ? new UploadDocumentConfig[] { new(DocumentTypeId.APP_CONTRACT, new[] { MediaTypeId.PDF }) }
            : new UploadDocumentConfig[] { new(DocumentTypeId.ADDITIONAL_DETAILS, new[] { MediaTypeId.PDF }) };
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, companyId, offerStatusId, offerTypeId))
            .Returns((true, false, true));

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, offerTypeId, uploadDocumentTypeIdSettings, offerStatusId, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"offerStatus is in Incorrect State");
    }

    #endregion

    private void SetupCreateDocument()
    {
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(_validAppId, _identity.CompanyId, OfferStatusId.CREATED, A<OfferTypeId>._))
            .Returns((true, true, true));
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }
}
