/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class DocumentsBusinessLogicTests
{
    private static readonly Guid ValidDocumentId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IDocumentRepository _documentRepository;
    private IPortalRepositories _portalRepositories;
    private IHostEnvironment _hostingEnvironment;

    public DocumentsBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _documentRepository = A.Fake<IDocumentRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _hostingEnvironment = A.Fake<IHostEnvironment>();
        _hostingEnvironment.EnvironmentName = "Development";
    }

    #region GetSeedData
    
    [Fact]
    public async Task GetSeedData_WithValidId_ReturnsValidData()
    {
        // Arrange
        SetupFakesForGetSeedData();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _hostingEnvironment);
        
        // Act
        var result = await sut.GetSeedData(ValidDocumentId).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        SetupFakesForGetSeedData();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _hostingEnvironment);
        
        // Act
        async Task Act() => await sut.GetSeedData(invalidId).ConfigureAwait(false);
        
        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"Document {invalidId} does not exists.");
    }
    
    [Fact]
    public async Task CreateConnectorAsync_WithCallFromTest_ThrowsForbiddenException()
    {
        // Arrange
        SetupFakesForGetSeedData();
        _hostingEnvironment.EnvironmentName = "Test";
        var sut = new DocumentsBusinessLogic(_portalRepositories, _hostingEnvironment);
        
        // Act
        async Task Act() => await sut.GetSeedData(ValidDocumentId).ConfigureAwait(false);
        
        // Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(Act);
        exception.Message.Should().Be("Endpoint can only be used on dev environment");
    }

    #endregion
    
    #region Setup

    private void SetupFakesForGetSeedData()
    {
        A.CallTo(() => _documentRepository.GetDocumentSeedDataByIdAsync(A<Guid>.That.Matches(x => x == ValidDocumentId)))
            .Returns(_fixture.Create<DocumentSeedData>());
        A.CallTo(() => _documentRepository.GetDocumentSeedDataByIdAsync(A<Guid>.That.Not.Matches(x => x == ValidDocumentId)))
            .Returns((DocumentSeedData?)null);

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}