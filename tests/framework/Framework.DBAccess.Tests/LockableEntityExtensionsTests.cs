/********************************************************************************
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess.Tests;

public class LockableEntityExtensionsTests
{
    private readonly DateTimeOffset _now;
    private readonly DateTimeOffset _within;
    private readonly DateTimeOffset _soon;
    private readonly DateTimeOffset _expired;
    private readonly IFixture _fixture;

    public LockableEntityExtensionsTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _now = _fixture.Create<DateTimeOffset>();
        _within = _now.AddMinutes(1);
        _soon = _within.AddMinutes(1);
        _expired = _soon.AddMinutes(1);
    }

    [Theory]
    [InlineData(TimeFrame.NULL, TimeFrame.SOON, true)]
    [InlineData(TimeFrame.SOON, TimeFrame.WITHIN, false)]
    [InlineData(TimeFrame.SOON, TimeFrame.SOON, false)]
    [InlineData(TimeFrame.SOON, TimeFrame.EXPIRED, false)]
    public void TryLock_ReturnsExpected(TimeFrame initialTimeFrame, TimeFrame lockTimeFrame, bool expected)
    {
        // Arrange
        var version = Guid.NewGuid();
        var initialDateTime = GetDateTimeOffset(initialTimeFrame);
        var entity = new Entity(version, initialDateTime);
        var lockTimeDate = GetDateTimeOffset(lockTimeFrame)!.Value;

        // Act
        var result = entity.TryLock(lockTimeDate);

        // Assert
        result.Should().Be(expected);
        if (expected)
        {
            entity.Version.Should().NotBe(version);
            entity.LockExpiryDate.Should().Be(lockTimeDate);
        }
        else
        {
            entity.Version.Should().Be(version);
            entity.LockExpiryDate.Should().Be(initialDateTime);
        }
    }

    [Theory]
    [InlineData(TimeFrame.NULL)]
    [InlineData(TimeFrame.SOON)]
    public void UpdateVersion_ReturnsExpected(TimeFrame initialTimeFrame)
    {
        // Arrange
        var version = Guid.NewGuid();
        var initialDateTime = GetDateTimeOffset(initialTimeFrame);
        var entity = new Entity(version, initialDateTime);

        // Act
        entity.UpdateVersion();

        // Assert
        entity.Version.Should().NotBe(version);
        entity.LockExpiryDate.Should().Be(initialDateTime);
    }

    [Theory]
    [InlineData(TimeFrame.NULL, false)]
    [InlineData(TimeFrame.SOON, true)]
    public void ReleaseLock_ReturnsExpected(TimeFrame initialTimeFrame, bool expected)
    {
        // Arrange
        var version = Guid.NewGuid();
        var initialDateTime = GetDateTimeOffset(initialTimeFrame);
        var entity = new Entity(version, initialDateTime);

        // Act
        var result = entity.ReleaseLock();

        // Assert
        result.Should().Be(expected);
        if (expected)
        {
            entity.Version.Should().NotBe(version);
            entity.LockExpiryDate.Should().BeNull();
        }
        else
        {
            entity.Version.Should().Be(version);
            entity.LockExpiryDate.Should().Be(initialDateTime);
        }
    }

    [Theory]
    [InlineData(TimeFrame.NULL, false)]
    [InlineData(TimeFrame.SOON, true)]
    public void IsLocked_ReturnsExpected(TimeFrame initialTimeFrame, bool expected)
    {
        // Arrange
        var version = Guid.NewGuid();
        var initialDateTime = GetDateTimeOffset(initialTimeFrame);
        var entity = new Entity(version, initialDateTime);

        // Act
        var result = entity.IsLocked();

        // Assert
        result.Should().Be(expected);

        entity.Version.Should().Be(version);
        entity.LockExpiryDate.Should().Be(initialDateTime);
    }

    [Theory]
    [InlineData(TimeFrame.NULL, TimeFrame.WITHIN, false)]
    [InlineData(TimeFrame.NULL, TimeFrame.SOON, false)]
    [InlineData(TimeFrame.NULL, TimeFrame.EXPIRED, false)]
    [InlineData(TimeFrame.SOON, TimeFrame.WITHIN, false)]
    [InlineData(TimeFrame.SOON, TimeFrame.SOON, false)]
    [InlineData(TimeFrame.SOON, TimeFrame.EXPIRED, true)]
    public void IsLockExpired_ReturnsExpected(TimeFrame initialTimeFrame, TimeFrame lockTimeFrame, bool expected)
    {
        // Arrange
        var version = Guid.NewGuid();
        var initialDateTime = GetDateTimeOffset(initialTimeFrame);
        var entity = new Entity(version, initialDateTime);
        var lockTimeDate = GetDateTimeOffset(lockTimeFrame)!.Value;

        // Act
        var result = entity.IsLockExpired(lockTimeDate);

        // Assert
        result.Should().Be(expected);

        entity.Version.Should().Be(version);
        entity.LockExpiryDate.Should().Be(initialDateTime);
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

    private class Entity : ILockableEntity
    {
        public Entity(Guid version, DateTimeOffset? lockExpiryDate)
        {
            Version = version;
            LockExpiryDate = lockExpiryDate;
        }
        public Guid Version { get; set; }
        public DateTimeOffset? LockExpiryDate { get; set; }
    }
}
