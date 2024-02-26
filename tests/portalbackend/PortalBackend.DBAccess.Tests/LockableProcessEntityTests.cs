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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class LockableProcessEntityTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly DateTimeOffset _now;
    private readonly DateTimeOffset _within;
    private readonly DateTimeOffset _soon;
    private readonly DateTimeOffset _expired;
    private readonly TestDbFixture _dbTestDbFixture;

    public LockableProcessEntityTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _now = DateTimeOffset.UtcNow;
        _within = _now.AddMinutes(1);
        _soon = _within.AddMinutes(1);
        _expired = _soon.AddMinutes(1);

        _dbTestDbFixture = testDbFixture;
    }

    [Fact]
    public async Task IsLocked_ReturnsExpected()
    {
        var (processId, _, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var result = process!.IsLocked();
        result.Should().BeFalse();
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task ReleaseLock_ReturnsExpected()
    {
        var (processId, _, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var result = process!.ReleaseLock();
        result.Should().BeFalse();
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task UpdateVersion_UpdateVersion_ReturnsExpected()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        process!.UpdateVersion();
        process!.Version.Should().NotBe(version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var updated = await context.FindAsync<Process>(processId);
        updated.Should().NotBeNull();
        updated!.Version.Should().Be(process.Version);
        updated.UpdateVersion();
        updated!.Version.Should().NotBe(process.Version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await context.FindAsync<Process>(processId);
        result.Should().NotBeNull();
        result!.Version.Should().Be(updated.Version);
    }

    [Fact]
    public async Task TryLock_IsLocked_ReturnsExpected()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var isLocked = process!.TryLock(_soon);
        isLocked.Should().BeTrue();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var locked = await context.FindAsync<Process>(processId);
        locked.Should().NotBeNull();
        locked!.Version.Should().Be(process.Version);
        var result = locked.IsLocked();
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(TimeFrame.NOW, false)]
    [InlineData(TimeFrame.WITHIN, false)]
    [InlineData(TimeFrame.SOON, false)]
    [InlineData(TimeFrame.EXPIRED, false)]
    public async Task IsLockExpired_ReturnsExpected(TimeFrame timeFrame, bool expected)
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        process!.Version.Should().Be(version);
        var result = process.IsLockExpired(GetDateTimeOffset(timeFrame)!.Value);
        result.Should().Be(expected);
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Theory]
    [InlineData(TimeFrame.NOW, false)]
    [InlineData(TimeFrame.WITHIN, false)]
    [InlineData(TimeFrame.EXPIRED, true)]
    public async Task TryLock_IsLockExpired_ReturnsExpected(TimeFrame timeFrame, bool expected)
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var isLocked = process!.TryLock(_soon);
        isLocked.Should().BeTrue();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var locked = await context.FindAsync<Process>(processId);
        locked.Should().NotBeNull();
        locked!.Version.Should().Be(process.Version);
        var result = locked.IsLockExpired(GetDateTimeOffset(timeFrame)!.Value);
        result.Should().Be(expected);
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task TryLock_TryLock_ReturnsExpected()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var isLocked = process!.TryLock(_soon);
        isLocked.Should().BeTrue();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var locked = await context.FindAsync<Process>(processId);
        locked.Should().NotBeNull();
        locked!.Version.Should().Be(process.Version);
        var result = locked.TryLock(_soon);
        result.Should().BeFalse();
        locked!.Version.Should().Be(process.Version);
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task TryLock_ReleaseLock_ReturnsExpected()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var isLocked = process!.TryLock(_soon);
        isLocked.Should().BeTrue();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var locked = await context.FindAsync<Process>(processId);
        locked.Should().NotBeNull();
        locked!.Version.Should().Be(process.Version);
        var isReleased = locked.ReleaseLock();
        isReleased.Should().BeTrue();
        locked!.Version.Should().NotBe(process.Version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UpdateWithParallelUpdate_Throws()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        process!.UpdateVersion();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var parallel = context.Attach(new Process(processId, default, version)).Entity;
        parallel!.Version.Should().NotBe(process.Version);
        parallel.UpdateVersion();
        parallel!.Version.Should().NotBe(version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task UpdateWithParallelLock_Throws()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        process!.UpdateVersion();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var parallel = context.Attach(new Process(processId, default, version)).Entity;
        parallel!.Version.Should().NotBe(process.Version);
        var isReLocked = parallel.TryLock(_soon);
        isReLocked.Should().BeTrue();
        parallel!.Version.Should().NotBe(version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task LockWithParallelUpdate_Throws()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var locked = process!.TryLock(_soon);
        locked.Should().BeTrue();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var parallel = context.Attach(new Process(processId, default, version)).Entity;
        parallel!.Version.Should().NotBe(process.Version);
        parallel.UpdateVersion();
        parallel!.Version.Should().NotBe(version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task LockWithParallelLock_Throws()
    {
        var (processId, version, context) = await CreateProcessAndContext();

        var process = await context.FindAsync<Process>(processId);
        process.Should().NotBeNull();
        var isLocked = process!.TryLock(_soon);
        isLocked.Should().BeTrue();
        process!.Version.Should().NotBe(version);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var parallel = context.Attach(new Process(processId, default, version)).Entity;
        parallel!.Version.Should().NotBe(process.Version);
        var isReLocked = parallel.TryLock(_soon);
        isReLocked.Should().BeTrue();
        parallel!.Version.Should().NotBe(version);
        context.ChangeTracker.HasChanges().Should().BeTrue();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();
    }

    public enum TimeFrame
    {
        NULL,
        NOW,
        WITHIN,
        SOON,
        EXPIRED
    }

    private DateTimeOffset? GetDateTimeOffset(TimeFrame timeFrame) =>
        timeFrame switch
        {
            TimeFrame.NULL => null,
            TimeFrame.NOW => _now,
            TimeFrame.WITHIN => _within,
            TimeFrame.SOON => _soon,
            TimeFrame.EXPIRED => _expired,
            _ => default
        };

    private async Task<(Guid Id, Guid Version, DbContext Context)> CreateProcessAndContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var process = context.Add(new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid())).Entity;
        await context.SaveChangesAsync().ConfigureAwait(false);
        context.ChangeTracker.Clear();
        return (process.Id, process.Version, context);
    }
}
