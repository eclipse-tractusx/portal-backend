/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class MailingInformationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly IFixture _fixture;
    private readonly Guid _processId = new("44927361-3766-4f07-9f18-860158880d86");
    public MailingInformationRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetMailingInformationForProcess

    [Fact]
    public async Task GetMailingInformationForProcess_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();
        var encryptionKey = Convert.FromHexString("7769F42A68708AD145CEE5F5FAFD8734B396C15660A28FE8C6F9BBDB1044986C");

        // Act
        var data = await sut.GetMailingInformationForProcess(_processId).ToListAsync();

        // Assert
        data.Should().ContainSingle().And.Satisfy(x => x.Template == "CredentialRejected" && x.EmailAddress == "test@email.de" && x.EncryptionMode == 1);

        var mailingInformation = data.Single();
        var mailParameters = JsonSerializer.Deserialize<Dictionary<string, string>>(CryptoHelper.Decrypt(mailingInformation.MailParameters, mailingInformation.InitializationVector, encryptionKey, CipherMode.CBC, PaddingMode.PKCS7));
        mailParameters.Should().HaveCount(2).And.Satisfy(
            x => x.Key == "userName" && x.Value == "tony stark",
            x => x.Key == "requestName" && x.Value == "Traceability Framework"
        );
    }

    [Fact]
    public async Task GetMailingInformationForProcess_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetMailingInformationForProcess(Guid.NewGuid()).ToListAsync();

        // Assert
        data.Should().BeEmpty();
    }

    #endregion

    #region CreateMailingInformation

    [Fact]
    public async Task CreateMailingInformation_WithValidData_Creates()
    {
        var (sut, context) = await CreateSutWithContext();

        var mailParameters = _fixture.CreateMany<byte>(64).ToArray();
        var initializationVector = _fixture.CreateMany<byte>(64).ToArray();

        var mailingInformation = sut.CreateMailingInformation(_processId, "test@email.de", "test mail", mailParameters, initializationVector, 1);

        // Assert
        mailingInformation.Id.Should().NotBeEmpty();
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<MailingInformation>()
            .Which.Should().Match<MailingInformation>(x =>
                x.Email == "test@email.de" &&
                x.ProcessId == _processId &&
                x.MailParameters.SequenceEqual(mailParameters) &&
                x.InitializationVector.SequenceEqual(initializationVector) &&
                x.EncryptionMode == 1);
    }

    #endregion

    #region AttachAndModifyMailingInformation

    [Fact]
    public async Task AttachAndModifyMailingInformation()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();
        var existingId = Guid.NewGuid();

        // Act
        sut.AttachAndModifyMailingInformation(existingId, information => { information.MailingStatusId = MailingStatusId.PENDING; }, information => { information.MailingStatusId = MailingStatusId.SENT; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle().And.AllSatisfy(x => x.Entity.Should().BeOfType<MailingInformation>()).And.Satisfy(
            x => x.State == EntityState.Modified && ((MailingInformation)x.Entity).Id == existingId && ((MailingInformation)x.Entity).MailingStatusId == MailingStatusId.SENT
        );
    }

    #endregion

    #region Setup

    private async Task<(IMailingInformationRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new MailingInformationRepository(context);
        return (sut, context);
    }

    private async Task<IMailingInformationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new MailingInformationRepository(context);
        return sut;
    }

    #endregion
}
