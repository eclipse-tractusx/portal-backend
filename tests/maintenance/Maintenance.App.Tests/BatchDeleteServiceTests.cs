/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Maintenance.App.Tests;

public class BatchDeleteServiceTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IDocumentRepository _documentRepository;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly BatchDeleteService _sut;
    private readonly IMockLogger<BatchDeleteService> _mockLogger;

    public BatchDeleteServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockLogger = A.Fake<IMockLogger<BatchDeleteService>>();
        ILogger<BatchDeleteService> logger = new MockLogger<BatchDeleteService>(_mockLogger);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _offerRepository = A.Fake<IOfferRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);

        var dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => dateTimeProvider.OffsetNow).Returns(DateTimeOffset.UtcNow);

        var options = Options.Create(new BatchDeleteServiceSettings { DeleteIntervalInDays = 5 });
        _sut = new BatchDeleteService(logger, options, _portalRepositories, dateTimeProvider);
    }

    [Fact]
    public async Task CleanupDocuments_WithoutMatchingDocuments_DoesNothing()
    {
        // Arrange
        A.CallTo(() => _documentRepository.GetDocumentDataForCleanup(A<DateTimeOffset>._))
            .Returns(Array.Empty<(Guid, IEnumerable<Guid>, IEnumerable<Guid>)>().ToAsyncEnumerable());

        // Act
        await _sut.CleanupDocuments(CancellationToken.None);

        // Assert
        A.CallTo(() => _agreementRepository.AttachAndModifyAgreements(A<IEnumerable<(Guid, Action<Agreement>?, Action<Agreement>)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid, Guid)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CleanupDocuments_WithMatchingDocuments_RemovesExpected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var agreementId1 = Guid.NewGuid();
        var agreementId2 = Guid.NewGuid();
        var offerId1 = Guid.NewGuid();
        var offerId2 = Guid.NewGuid();
        A.CallTo(() => _documentRepository.GetDocumentDataForCleanup(A<DateTimeOffset>._))
            .Returns(new (Guid, IEnumerable<Guid>, IEnumerable<Guid>)[] { (documentId, [agreementId1, agreementId2], [offerId1, offerId2]) }.ToAsyncEnumerable());

        // Act
        await _sut.CleanupDocuments(CancellationToken.None);

        // Assert
        A.CallTo(() => _agreementRepository.AttachAndModifyAgreements(A<IEnumerable<(Guid Id, Action<Agreement>?, Action<Agreement>)>>.That.Matches(x => x.Count(a => a.Id == agreementId1) == 1 && x.Count(a => a.Id == agreementId2) == 1)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid OfferId, Guid DocumentId)>>.That.Matches(x => x.Count(a => a.OfferId == offerId1 && a.DocumentId == documentId) == 1 && x.Count(a => a.OfferId == offerId2 && a.DocumentId == documentId) == 1)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>.That.Matches(x => x.Single() == documentId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CleanupDocuments_WithException_LogsException()
    {
        // Arrange
        A.CallTo(() => _documentRepository.GetDocumentDataForCleanup(A<DateTimeOffset>._))
            .Throws(new DataMisalignedException("Test message"));

        // Act
        await _sut.CleanupDocuments(CancellationToken.None);

        // Assert
        A.CallTo(() => _agreementRepository.AttachAndModifyAgreements(A<IEnumerable<(Guid Id, Action<Agreement>?, Action<Agreement>)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid, Guid)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustHaveHappenedOnceExactly();
    }
}
