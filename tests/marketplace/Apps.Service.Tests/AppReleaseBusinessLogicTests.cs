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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.Tests;

public class AppReleaseBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IAppReleaseRepository _appReleaseRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly CompanyUser _companyUser;
    private readonly IamUser _iamUser;

    public AppReleaseBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _appReleaseRepository = A.Fake<IAppReleaseRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
            
        _companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        _iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, _companyUser)
            .Create();
        _companyUser.IamUser = _iamUser;
    }

    #region Create App Document
        
    [Fact]
    public async Task AddFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        SetupRepositories(appId);
        var documents = new List<Document>();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._,A<Action<Document>?>._))
            .Invokes(x =>
            {
                var documentName = x.Arguments.Get<string>("documentName")!;
                var documentContent = x.Arguments.Get<byte[]>("documentContent")!;
                var hash = x.Arguments.Get<byte[]>("hash")!;
                var action = x.Arguments.Get<Action<Document?>>("setupOptionalFields");

                var document = new Document(documentId, documentContent, hash, documentName, DateTimeOffset.UtcNow, DocumentStatusId.PENDING);
                action?.Invoke(document);
                documents.Add(document);
            });
        var offerAssignedDocuments = new List<OfferAssignedDocument>();
        A.CallTo(() => _appReleaseRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var offerId = x.Arguments.Get<Guid>("offerId");
                var docId = x.Arguments.Get<Guid>("documentId");

                var offerAssignedDocument = new OfferAssignedDocument(offerId, docId);
                offerAssignedDocuments.Add(offerAssignedDocument);
            });

        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<AppReleaseBusinessLogic>();

        // Act
        await sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
        offerAssignedDocuments.Should().HaveCount(1);
    }

    #endregion
        
    #region Setup
        
    private void SetupRepositories(Guid appId)
    {
        A.CallTo(() => _appReleaseRepository.GetCompanyUserIdForOfferUntrackedAsync(appId, _iamUser.UserEntityId))
            .Returns(_companyUser.Id);
        A.CallTo(() => _portalRepositories.GetInstance<IAppReleaseRepository>()).Returns(_appReleaseRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}