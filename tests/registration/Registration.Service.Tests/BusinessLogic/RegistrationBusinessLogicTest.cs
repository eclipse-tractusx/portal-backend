/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.BusinessLogic;

public class RegistrationBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRoleRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly string _iamUserId;
    private readonly Guid _companyUserId;
    private readonly Guid _existingApplicationId;
    private readonly string _displayName;
    private readonly TestException _error;
    private readonly IOptions<RegistrationSettings> _options;
    private readonly IMailingService _mailingService;
    private readonly Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;

    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _mailingService = A.Fake<IMailingService>();
        _invitationRepository = A.Fake<IInvitationRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRoleRepository = A.Fake<IUserRolesRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();

        var options = Options.Create(new RegistrationSettings
        {
            BasePortalAddress = "just a test",
            KeyCloakClientID = "CatenaX"
        });
        _fixture.Inject(options);
        _fixture.Inject(A.Fake<IMailingService>());
        _fixture.Inject(A.Fake<IBpnAccess>());
        _fixture.Inject(A.Fake<ILogger<RegistrationBusinessLogic>>());

        _options = _fixture.Create<IOptions<RegistrationSettings>>();

        _iamUserId = _fixture.Create<string>();
        _companyUserId = _fixture.Create<Guid>();
        _existingApplicationId = _fixture.Create<Guid>();
        _displayName = _fixture.Create<string>();
        _error = _fixture.Create<TestException>();

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

        SetupRepositories();
        
        _fixture.Inject(_provisioningManager);
        _fixture.Inject(_userProvisioningService);
        _fixture.Inject(_portalRepositories);
    }

    #region GetClientRolesComposite
    
    [Fact]
    public async Task GetClientRolesCompositeAsync_GetsAllRoles()
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        
        // Act
        var result = sut.GetClientRolesCompositeAsync();
        await foreach (var item in result)
        {
            // Assert
            A.CallTo(() => _userRoleRepository.GetClientRolesCompositeAsync(A<string>._)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
        }
    }
    
    #endregion
    
    #region GetCompanyByIdentifier
    
    [Fact]
    public async Task GetCompanyByIdentifierAsync_WithValidBpn_FetchesBusinessPartner()
    {
        //Arrange
        var bpnAccess = A.Fake<IBpnAccess>();
        var bpn = "THISBPNISVALID12";
        var token = "justatoken";
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            bpnAccess,
            null!,
            null!,
            null!,
            null!);

        // Act
        var result = await sut.GetCompanyByIdentifierAsync(bpn, token, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        result.Should().NotBeNull();
        A.CallTo(() => bpnAccess.FetchBusinessPartner(bpn, token, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task GetCompanyByIdentifierAsync_WithValidBpn_ThrowsArgumentException()
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
        
        // Act
        async Task Act() => await sut.GetCompanyByIdentifierAsync("NotLongEnough", "justatoken", CancellationToken.None).ToListAsync().ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        ex.ParamName.Should().Be("companyIdentifier");
    }

    #endregion
    
    #region GetAllApplicationsForUserWithStatus

    [Fact]
    public async Task GetAllApplicationsForUserWithStatus_WithValidUser_GetsAllRoles()
    {
        //Arrange
        var userId = _fixture.Create<string>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        var resultList = new List<CompanyApplicationWithStatus>
        {
            new()
            {
                ApplicationId = _fixture.Create<Guid>(),
                ApplicationStatus = CompanyApplicationStatusId.VERIFY
            }
        };
        A.CallTo(() => _userRepository.GetApplicationsWithStatusUntrackedAsync(userId))
            .Returns(resultList.ToAsyncEnumerable());

        // Act
        var result = await sut.GetAllApplicationsForUserWithStatus(userId).ToListAsync().ConfigureAwait(false);
        result.Should().ContainSingle();
        result.Single().ApplicationStatus.Should().Be(CompanyApplicationStatusId.VERIFY);
    }

    #endregion
    
    #region GetCompanyWithAddress
    
    [Fact]
    public async Task GetCompanyWithAddressAsync_WithValidApplication_GetsData()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        
        A.CallTo(() => _applicationRepository.GetCompanyWithAdressUntrackedAsync(applicationId))
            .ReturnsLazily(A.Fake<CompanyWithAddress>);

        // Act
        var result = await sut.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetCompanyWithAddressAsync_WithInvalidApplication_ThrowsNotFoundException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        
        A.CallTo(() => _applicationRepository.GetCompanyWithAdressUntrackedAsync(applicationId))
            .ReturnsLazily(() => (CompanyWithAddress?)null);

        // Act
        async Task Act() => await sut.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} not found");
    }

    #endregion
    
    #region SetCompanyWithAddress
    
    [Theory]
    [InlineData(null, null, null, null, "Name")]
    [InlineData("filled", null, null, null, "City")]
    [InlineData("filled", "filled", null, null, "StreetName")]
    [InlineData("filled", "filled", "filled", "", "CountryAlpha2Code")]
    public async Task SetCompanyWithAddressAsync_WithMissingData_ThrowsArgumentException(string? name, string? city, string? streetName, string? countryCode, string argumentName)
    {
        //Arrange
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
        var companyData = new CompanyWithAddress(Guid.NewGuid(), name!, city!, streetName!, countryCode!);

        // Act
        async Task Act() => await sut.SetCompanyWithAddressAsync(Guid.NewGuid(), companyData, string.Empty).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be(argumentName);
    }
    
    [Fact]
    public async Task SetCompanyWithAddressAsync_WithInvalidApplicationId_ThrowsNotFoundException()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        var companyData = new CompanyWithAddress(companyId, "name", "munich", "main street", "de");

        A.CallTo(() => _applicationRepository.GetCompanyApplicationWithCompanyAdressUserDataAsync(applicationId, companyId, A<string>._))
            .ReturnsLazily(() => (CompanyApplicationWithCompanyAddressUserData?)null);
        
        // Act
        async Task Act() => await sut.SetCompanyWithAddressAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} for CompanyId {companyId} not found");
    }
    
    [Fact]
    public async Task SetCompanyWithAddressAsync_WithoutCompanyUserId_ThrowsForbiddenException()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        var companyData = new CompanyWithAddress(companyId, "name", "munich", "main street", "de");

        A.CallTo(() => _applicationRepository.GetCompanyApplicationWithCompanyAdressUserDataAsync(applicationId, companyId, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationWithCompanyAddressUserData(A.Fake<CompanyApplication>()));
        
        // Act
        async Task Act() => await sut.SetCompanyWithAddressAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain($" is not assigned with CompanyApplication {applicationId}");
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithoutCompanyAddress_CreatesAddress()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyApplication = A.Fake<CompanyApplication>();
        companyApplication.Company = A.Fake<PortalBackend.PortalEntities.Entities.Company>();
        companyApplication.Company.Address = null;
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        var companyData = new CompanyWithAddress(companyId, "name", "munich", "main street", "de");

        A.CallTo(() => _applicationRepository.GetCompanyApplicationWithCompanyAdressUserDataAsync(applicationId, companyId, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationWithCompanyAddressUserData(companyApplication)
            {
                CompanyUserId = _fixture.Create<Guid>()
            });
        
        // Act
        await sut.SetCompanyWithAddressAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetCompanyWithAddressAsync_WithCompanyAddress_DoesntCreateAddress()
    {
        //Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyApplication = _fixture.Create<CompanyApplication>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        var companyData = new CompanyWithAddress(companyId, "name", "munich", "main street", "de");

        A.CallTo(() => _applicationRepository.GetCompanyApplicationWithCompanyAdressUserDataAsync(applicationId, companyId, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationWithCompanyAddressUserData(companyApplication)
            {
                CompanyUserId = _fixture.Create<Guid>()
            });
        
        // Act
        await sut.SetCompanyWithAddressAsync(applicationId, companyData, string.Empty).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateAddress(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region SetOwnCompanyApplicationStatus
    
    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidStatus_ThrowsControllerArgumentException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, 0, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("status must not be null");
    }
    
    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidApplication_ThrowsNotFoundException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => (CompanyApplicationUserData?) null);
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"CompanyApplication {applicationId} not found");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithoutAssignedUser_ThrowsForbiddenException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationUserData(_fixture.Create<CompanyApplication>()) );
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain($"is not associated with application {applicationId}");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithInvalidStatus_ThrowsArgumentException()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.CREATED)
            .Create();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationUserData(companyApplication)
            {
                CompanyUserId = _fixture.Create<Guid>()
            });
        
        // Act
        async Task Act() => await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.VERIFY, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Contain("invalid status update requested");
    }

    [Fact]
    public async Task SetOwnCompanyApplicationStatusAsync_WithValidData_SavesChanges()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var companyApplication = _fixture.Build<CompanyApplication>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.VERIFY)
            .Create();
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        A.CallTo(() => _applicationRepository.GetOwnCompanyApplicationUserDataAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => new CompanyApplicationUserData(companyApplication)
            {
                CompanyUserId = _fixture.Create<Guid>()
            });
        
        // Act
        await sut.SetOwnCompanyApplicationStatusAsync(applicationId, CompanyApplicationStatusId.SUBMITTED, _fixture.Create<string>()).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region GetCompanyRoles
    
    [Fact]
    public async Task GetCompanyRolesAsync_()
    {
        //Arrange
        var companyRolesRepository = A.Fake<ICompanyRolesRepository>();
        A.CallTo(() => companyRolesRepository.GetCompanyRolesAsync(A<string?>._))
            .Returns(_fixture.CreateMany<CompanyRolesDetails>(2).ToAsyncEnumerable());
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRolesRepository>())
            .Returns(companyRolesRepository);
        var sut = new RegistrationBusinessLogic(
            _options,
            null!,
            null!,
            null!,
            null!,
            null!,
            _portalRepositories);
        // Act
        var result = await sut.GetCompanyRoles().ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
    }
    
    #endregion
    
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
    
    #region UploadDocument
    
    [Fact]
    public async Task UploadDocumentAsync_WithValidData_CreatesDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var documents = new List<Document>();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<DocumentTypeId>._,A<Action<Document>?>._))
            .Invokes(x =>
            {
                var documentName = x.Arguments.Get<string>("documentName")!;
                var documentContent = x.Arguments.Get<byte[]>("documentContent")!;
                var hash = x.Arguments.Get<byte[]>("hash")!;
                var documentTypeId = x.Arguments.Get<DocumentTypeId>("documentType")!;
                var action = x.Arguments.Get<Action<Document?>>("setupOptionalFields");

                var document = new Document(documentId, documentContent, hash, documentName, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                action?.Invoke(document);
                documents.Add(document);
            });
        var sut = _fixture.Create<RegistrationBusinessLogic>();

        // Act
        await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None);

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
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None).ConfigureAwait(false);

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
        async Task Action() => await sut.UploadDocumentAsync(_existingApplicationId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None);

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
        async Task Action() => await sut.UploadDocumentAsync(notExistingId, file, DocumentTypeId.ADDITIONAL_DETAILS, _iamUserId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"iamUserId {_iamUserId} is not assigned with CompanyApplication {notExistingId}");
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
        ex.Message.Should().Be($"iamUserId {notExistingId} is not assigned with CompanyApplication {_existingApplicationId}");
    }

    #endregion

    #region InviteNewUser

    [Fact]
    public async Task TestInviteNewUserAsyncSuccess()
    {
        SetupFakesForInvitation();

        var userCreationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories);

        await sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId).ConfigureAwait(false);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_existingApplicationId),A<Guid>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail), A<IDictionary<string,string>>.That.Matches(x => x["companyName"] == _displayName), A<List<string>>._)).MustHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserEmptyEmailThrows()
    {
        SetupFakesForInvitation();

        var userCreationInfo = _fixture.Build<UserCreationInfoWithMessage>()
            .With(x => x.eMail, "")
            .Create();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories);

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("email must not be empty");

        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string,string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserUserAlreadyExistsThrows()
    {
        SetupFakesForInvitation();

        A.CallTo(() => _userRepository.IsOwnCompanyUserWithEmailExisting(A<string>._,A<string>._)).Returns(true);

        var userCreationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories);

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user with email {userCreationInfo.eMail} does already exist");

        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string,string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestInviteNewUserAsyncCreationErrorThrows()
    {
        SetupFakesForInvitation();

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, _error)
                .Create());

        var userCreationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        var sut = new RegistrationBusinessLogic(
            _options,
            _mailingService,
            null!,
            _provisioningManager,
            _userProvisioningService,
            null!,
            _portalRepositories);

        Task Act() => sut.InviteNewUserAsync(_existingApplicationId, userCreationInfo, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_existingApplicationId),A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail), A<IDictionary<string,string>>._, A<List<string>>._)).MustNotHaveHappened();
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
                A<Guid>.That.Matches(x => x == _existingApplicationId), A<string>.That.Matches(x => x == _iamUserId)))
            .ReturnsLazily(() => _companyUserId);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Matches(x => x == _existingApplicationId), A<string>.That.Not.Matches(x => x == _iamUserId)))
            .ReturnsLazily(() => Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), A<string>.That.Matches(x => x == _iamUserId)))
            .ReturnsLazily(() => Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForUserApplicationUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == _existingApplicationId), A<string>.That.Not.Matches(x => x == _iamUserId)))
            .ReturnsLazily(() => Guid.Empty);
        
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>())
            .Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IInvitationRepository>())
            .Returns(_invitationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>())
            .Returns(_applicationRepository);
    }

    private void SetupFakesForInvitation()
    {
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IDictionary<string,IEnumerable<string>>>._))
            .ReturnsLazily((IDictionary<string,IEnumerable<string>> clientRoles) =>
                clientRoles.SelectMany(r => r.Value.Select(role => _fixture.Build<UserRoleData>().With(x => x.UserRoleText, role).Create())).ToAsyncEnumerable());

        A.CallTo(() => _userProvisioningService.GetIdentityProviderDisplayName(A<string>._)).Returns(_displayName);

        A.CallTo(() => _userProvisioningService.GetCompanyNameSharedIdpAliasData(A<string>._,A<Guid?>._)).Returns(
            (
                _fixture.Create<CompanyNameIdpAliasData>(),
                _fixture.Create<string>()
            ));

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, (Exception?)null)
                .Create());
    }

    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
