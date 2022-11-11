/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Notification.Library;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppReleaseBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IOptions<AppsSettings> _options;
    private readonly AppReleaseBusinessLogic _logic;
    private readonly CompanyUser _companyUser;
    private readonly IamUser _iamUser;
    private readonly AppsSettings _settings;
    private readonly IFormFile _document;
    private readonly IOfferService _offerService;
    private readonly INotificationService _notificationService;
    public AppReleaseBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _offerService = A.Fake<IOfferService>();
        _notificationService = A.Fake<INotificationService>();
        _settings = A.Fake<AppsSettings>();
        _document = A.Fake<IFormFile>();
        _options = A.Fake<IOptions<AppsSettings>>();
        _companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        _iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, _companyUser)
            .Create();
        _companyUser.IamUser = _iamUser;
        A.CallTo(() => _options.Value).Returns(_settings);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _logic = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService, _notificationService);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var appUserRoleDescription = new [] {
            new AppUserRoleDescription("en","this is test1"),
            new AppUserRoleDescription("de","this is test2"),
            new AppUserRoleDescription("fr","this is test3")
        };
        var appUserRoles = new [] {
            new AppUserRole("IT Admin",appUserRoleDescription)
        };

        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>.That.IsEqualTo(appId), A<string>.That.IsEqualTo(iamUserId), A<OfferTypeId>.That.IsEqualTo(OfferTypeId.APP))).Returns((true,true));

        // Act
        var result = await _logic.AddAppUserRoleAsync(appId, appUserRoles, iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>._, A<string>._, A<OfferTypeId>._)).MustHaveHappened();
        foreach(var appRole in appUserRoles)
        {
            A.CallTo(() => _userRolesRepository.CreateAppUserRole(A<Guid>._, A<string>.That.IsEqualTo(appRole.role))).MustHaveHappened();
            foreach(var item in appRole.descriptions)
            {
                A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescription(A<Guid>._, A<string>.That.IsEqualTo(item.languageCode), A<string>.That.IsEqualTo(item.description))).MustHaveHappened();
            }
        }

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<AppRoleData>>(result);
    }
    
    #region Create App Document
        
    [Fact]
    public async Task CreateAppDocumentAsync_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        SetupRepositories(appId);
        var documents = new List<Document>();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<DocumentTypeId>._,A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentType, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentType);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            });
        var offerAssignedDocuments = new List<OfferAssignedDocument>();
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid documentId) =>
            {
                var offerAssignedDocument = new OfferAssignedDocument(offerId, documentId);
                offerAssignedDocuments.Add(offerAssignedDocument);
            });
        _settings.ContentTypeSettings = new [] { "application/pdf" };
        _settings.DocumentTypeIds = new [] { DocumentTypeId.APP_CONTRACT };

        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService, _notificationService);

        // Act
        await sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
        offerAssignedDocuments.Should().HaveCount(1);
    }
  
    #endregion
    
    [Fact]
    public async Task CreateAppDocumentAsync_WrongContentTypeThrows()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        SetupRepositories(appId);
        var documents = new List<Document>();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<DocumentTypeId>._,A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentType, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentType);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            });
        var offerAssignedDocuments = new List<OfferAssignedDocument>();
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid documentId) =>
            {
                var offerAssignedDocument = new OfferAssignedDocument(offerId, documentId);
                offerAssignedDocuments.Add(offerAssignedDocument);
            });
        
        _settings.ContentTypeSettings = new [] { "text/csv" };
        _settings.DocumentTypeIds = new [] { DocumentTypeId.APP_CONTRACT };
        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService, _notificationService);
     
        // Act
        async Task Act() => await sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        
        var error = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
       
        error.Message.Should().Be($"Document type not supported. File with contentType :{string.Join(",", _settings.ContentTypeSettings)} are allowed.");
    }

    #region Setup
        
    private void SetupRepositories(Guid appId)
    {
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(appId, _iamUser.UserEntityId, OfferStatusId.CREATED, OfferTypeId.APP))
            .ReturnsLazily(() => (true, _companyUser.Id));
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}
