/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatenaX.NetworkServices.Registration.Service.Tests.BusinessLogic;

public class RegistrationBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPortalRepositories _portalRepositories;
    private const string IamUserId = "807b4089-5258-496b-974d-feeb5540cb50";
    private readonly Guid _companyUserId = new ("807b4089-5258-496b-974d-feeb5540cb99");
    private readonly Guid _existingApplicationId = Guid.NewGuid();

    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _invitationRepository = A.Fake<IInvitationRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _userRepository = A.Fake<IUserRepository>();

        var options = Options.Create(new RegistrationSettings
        {
            BasePortalAddress = "just a test",
            KeyCloakClientID = "CatenaX"
        });
        _fixture.Inject(options);
        _fixture.Inject(A.Fake<IMailingService>());
        _fixture.Inject(A.Fake<IBPNAccess>());
        _fixture.Inject(A.Fake<ILogger<RegistrationBusinessLogic>>());

        SetupRepositories();
        
        _fixture.Inject(_provisioningManager);
        _fixture.Inject(_portalRepositories);

    }

    #region GetInvitedUser
    
    [Fact]
    public async Task Get_WhenThereAreInvitedUser_ShouldReturnInvitedUserWithRoles()
    {
        //Arrange
        var sut = _fixture.Create<RegistrationBusinessLogic>();
        
        //Act
        var result = sut.GetInvitedUsersAsync(_existingApplicationId);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(_existingApplicationId)).MustHaveHappened(1, Times.OrMore);
            A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._)).MustHaveHappened(1, Times.OrMore);
            Assert.NotNull(item);
            Assert.IsType<InvitedUser>(item);
        }
    }

    [Fact]
    public async Task GetInvitedUsersDetail_ThrowException_WhenIdIsNull()
    {
        //Arrange
        var sut = _fixture.Create<RegistrationBusinessLogic>();
        
        //Act
        async Task Action() => await sut.GetInvitedUsersAsync(Guid.Empty).ToListAsync().ConfigureAwait(false);
        
        // Assert
        await Assert.ThrowsAsync<Exception>(Action);
    }
    
    #endregion
    
    #region UploadDocumentAsync
    
    [Fact]
    public async Task UploadDocumentAsync_WithValidData_CreatesDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
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
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, IamUserId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithJsonDocument_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.json", "application/json");
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Action);
        ex.Message.Should().Be("Only .pdf files are allowed.");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithEmptyTitle_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", string.Empty, "application/pdf");
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, IamUserId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be("File name is must not be null");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNotExistingApplicationId_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var sut = _fixture.Create<RegistrationBusinessLogic>();
        var notExistingId = Guid.NewGuid();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(notExistingId, file, DocumentTypeId.ADDITIONAL_DETAILS, IamUserId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"iamUserId {IamUserId} is not assigned with CompanyAppication {notExistingId}");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithNotExistingIamUser_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var sut = _fixture.Create<RegistrationBusinessLogic>();
        var notExistingId = Guid.NewGuid();

        // Act
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, notExistingId.ToString(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"iamUserId {notExistingId} is not assigned with CompanyAppication {_existingApplicationId}");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        var invitedUserRole = _fixture.CreateMany<string>(2).AsEnumerable();
        var invitedUser = _fixture.CreateMany<InvitedUserDetail>(1).ToAsyncEnumerable();
        A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(_existingApplicationId))
            .Returns(invitedUser);
        A.CallTo(() => _invitationRepository.GetInvitedUserDetailsUntrackedAsync(Guid.Empty)).Throws(new Exception());
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(A<string>._, A<string>._))
            .Returns(invitedUserRole);
        A.CallTo(() => _provisioningManager.GetClientRoleMappingsForUserAsync(string.Empty, string.Empty)).Throws(new Exception());

        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Matches(x => x == _existingApplicationId), A<string>.That.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => _companyUserId);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Matches(x => x == _existingApplicationId), A<string>.That.Not.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), A<string>.That.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), A<string>.That.Not.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => Guid.Empty);
        
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>())
            .Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IInvitationRepository>())
            .Returns(_invitationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);

    }

    #endregion
}
