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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

namespace Offer.Library.Web.Tests;

public class OfferDocumentServiceTests
{
    private static readonly Guid CompanyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _validAppId = Guid.NewGuid();
    private readonly IdentityData _identity = new("395f955b-f11b-4a55-ab51-92a526c1974b", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, CompanyUserCompanyId);

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly OfferDocumentService _sut;

    public OfferDocumentServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        SetupCreateDocument();

        _sut = new OfferDocumentService(_portalRepositories);
    }

    #region UploadDocument

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS)]
    public async Task UploadDocumentAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
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

        // Act
        await _sut.UploadDocumentAsync(_validAppId, documentTypeId, file, (_identity.UserId, _identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
        offerAssignedDocuments.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS)]
    public async Task UploadDocumentAsync_InValidData_ThrowsNotFoundException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, _identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns(((bool, bool, bool))default);

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, (_identity.UserId, _identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} {id} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS)]
    public async Task UploadDocumentAsync_EmptyId_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(Guid.Empty, documentTypeId, file, (_identity.UserId, _identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} id should not be null");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS)]
    public async Task UploadDocumentAsync_EmptyFileName_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
        var file = FormFileHelper.GetFormFile("this is just a test", "", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, (_identity.UserId, _identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"File name should not be null");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS)]
    public async Task UploadDocumentAsync_contentType_ThrowsUnsupportedMediaTypeException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
        var file = FormFileHelper.GetFormFile("this is just a test", "TestFile.txt", "text/csv");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, (_identity.UserId, _identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Document type {documentTypeId} is not supported. File with contentType :{string.Join(",", uploadDocumentTypeIdSettings.Where(x => x.Key == documentTypeId).Select(x => x.Value).First())} are allowed.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.SELF_DESCRIPTION)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.APP_TECHNICAL_INFORMATION)]
    public async Task UploadDocumentAsync_documentType_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, (_identity.UserId, _identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"documentType must be either: {string.Join(",", uploadDocumentTypeIdSettings.Keys)}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS)]
    public async Task UploadDocumentAsync_isStatusCreated_ThrowsConflictException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var identity = _fixture.Create<IdentityData>();
        var uploadDocumentTypeIdSettings = offerTypeId == OfferTypeId.APP ? new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.APP_CONTRACT, new []{ "application/pdf" }}} : new Dictionary<DocumentTypeId, IEnumerable<string>> {
            {DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" }}};
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns((true, false, true));

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, (identity.UserId, identity.CompanyId), offerTypeId, uploadDocumentTypeIdSettings, CancellationToken.None).ConfigureAwait(false);

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
