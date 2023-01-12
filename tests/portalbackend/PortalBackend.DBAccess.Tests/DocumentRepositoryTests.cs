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
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="DocumentRepositoryTests"/>
/// </summary>
public class DocumentRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public DocumentRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create Document

    [Fact]
    public async Task CreateDocument_ReturnsExpectedDocument()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);
        var test = "This is just test content";
        var content = Encoding.UTF8.GetBytes(test);

        // Act
        var result = sut.CreateDocument("New Document", content, content, DocumentTypeId.APP_DATA_DETAILS, doc =>
        {
            doc.DocumentStatusId = DocumentStatusId.INACTIVE;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.DocumentTypeId.Should().Be(DocumentTypeId.APP_DATA_DETAILS);
        result.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Added);
    }
    
    #endregion

    #region GetUploadedDocuments

    [Theory]
    [InlineData("4829b64c-de6a-426c-81fc-c0bcf95bcb76", DocumentTypeId.CX_FRAME_CONTRACT, "623770c5-cf38-4b9f-9a35-f8b9ae972e2e", 2)]
    [InlineData("1b86d973-3aac-4dcd-a9e9-0c222766202b", DocumentTypeId.CX_FRAME_CONTRACT, "4b8f156e-5dfc-4a58-9384-1efb195c1c34", 1)]
    [InlineData("1b86d973-3aac-4dcd-a9e9-0c222766202b", DocumentTypeId.APP_CONTRACT, "4b8f156e-5dfc-4a58-9384-1efb195c1c34", 0)]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedDocuments(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId, int count)
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var results = await sut.GetUploadedDocumentsAsync(applicationId, documentTypeId, iamUserId).ConfigureAwait(false);
    
        // Assert
        results.Should().NotBe(default);
        results.IsApplicationAssignedUser.Should().BeTrue();
        results.Documents.Should().HaveCount(count);
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_InvalidApplication_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var result = await sut.GetUploadedDocumentsAsync(Guid.NewGuid(), DocumentTypeId.CX_FRAME_CONTRACT, "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_InvalidUser_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var result = await sut.GetUploadedDocumentsAsync(new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), DocumentTypeId.CX_FRAME_CONTRACT, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.IsApplicationAssignedUser.Should().BeFalse();
        result.Documents.Should().BeEmpty();
    }

    #endregion

    #region AttachAndModifyDocument

    [Fact]
    public async Task AttachAndModifyDocument_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyDocument(Guid.NewGuid(),
            null,
            docstatusId =>{ docstatusId.DocumentStatusId = DocumentStatusId.LOCKED; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should()
            .BeOfType<PortalEntities.Entities.Document>()
                .Which.DocumentStatusId.Should().Be(DocumentStatusId.LOCKED);
    }

    [Fact]
    public async Task AttachAndModifyDocument_NoUpdate()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyDocument(Guid.NewGuid(),
            docstatusId =>{ docstatusId.DocumentStatusId = DocumentStatusId.LOCKED; },
            docstatusId =>{ docstatusId.DocumentStatusId = DocumentStatusId.LOCKED; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
    }

    #endregion

    #region Seed Data

    [Fact]
    public async Task GetDocumentSeedDataByIdAsync_ReturnsExpectedDocuments()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var results = await sut.GetDocumentSeedDataByIdAsync(new Guid("fda6c9cb-62be-4a98-99c1-d9c5a2df4aad")).ConfigureAwait(false);
    
        // Assert
        results.Should().NotBeNull();
        results!.DocumentStatusId.Should().Be(3);
        results.DocumentTypeId.Should().Be(1);
        results.DocumentName.Should().Be("test1.pdf");
    }

    [Fact]
    public async Task GetDocumentSeedDataByIdAsync_NotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var result = await sut.GetDocumentSeedDataByIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAppImageDocumentContentAsync_ReturnsExpectedResult()
    {
        // Arrange
        IEnumerable<DocumentTypeId> docIds = new List<DocumentTypeId>{DocumentTypeId.APP_IMAGE,DocumentTypeId.APP_LEADIMAGE};
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var result = await sut.GetAppImageDocumentContentAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"),new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea41"),docIds).ConfigureAwait(false);

        // Assert
        result.IsDocumentTypeIdLeadImage.Should().Be(true);
    }

    #endregion
    
    private async Task<(DocumentRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new DocumentRepository(context);
        return (sut, context);
    }
}
