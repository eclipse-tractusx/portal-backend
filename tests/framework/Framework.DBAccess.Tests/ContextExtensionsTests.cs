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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess.Tests;

public class ContextExtensionsTests
{
	private readonly IFixture _fixture;
	private readonly DbContext _context;

	public ContextExtensionsTests()
	{
		_fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
		_fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
			.ForEach(b => _fixture.Behaviors.Remove(b));
		_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

		_context = A.Fake<DbContext>();
	}

	#region AddRemoveRange

	[Theory]
	[InlineData(
		new[] { "1e237b5f-f486-4583-b121-0885593c8242", "df27919c-30c6-4ee7-8356-7d9fef01e2a9", "f1166064-2457-458e-b8f9-32a481f34785", "7dda58bd-cfe5-4f0f-8070-ea57a8371c78", "10ed72c8-492b-4569-9448-8f438cfc6b55" }, // initialKeys
		new[] { "1e237b5f-f486-4583-b121-0885593c8242", "7f11d9a9-5ab3-4691-8a11-6250e72aa31d", "f1166064-2457-458e-b8f9-32a481f34785", "6f00c68f-85ca-47ba-b8ba-8fe8aa58b53d", "10ed72c8-492b-4569-9448-8f438cfc6b55" }, // updateKeys
		new[] { "7f11d9a9-5ab3-4691-8a11-6250e72aa31d", "6f00c68f-85ca-47ba-b8ba-8fe8aa58b53d" }, // addedEntityKeys
		new[] { "df27919c-30c6-4ee7-8356-7d9fef01e2a9", "7dda58bd-cfe5-4f0f-8070-ea57a8371c78" }  // removedEntityKeys
	)]

	public void AddRemoveRangeSuccess(
		IEnumerable<string> initialKeys,
		IEnumerable<string> updateKeys,
		IEnumerable<string> addedEntityKeys,
		IEnumerable<string> removedEntityKeys)
	{
		var key1 = Guid.NewGuid();
		var initial = initialKeys.Select(x => new Guid(x)).ToImmutableArray();
		var update = updateKeys.Select(x => new Guid(x)).ToImmutableArray();
		var addedEntities = addedEntityKeys.Select(x => new TestEntity(key1, new Guid(x))).OrderBy(x => x.EntityKey2).ToImmutableArray();
		var removedEntities = removedEntityKeys.Select(x => new TestEntity(key1, new Guid(x))).OrderBy(x => x.EntityKey2).ToImmutableArray();

		IEnumerable<TestEntity>? added = null;
		IEnumerable<TestEntity>? removed = null;

		A.CallTo(() => _context.AddRange(A<IEnumerable<TestEntity>>._))
			.Invokes((IEnumerable<object> entities) =>
			{
				added = entities.Select(x => (TestEntity)x).ToImmutableArray();
			});
		A.CallTo(() => _context.RemoveRange(A<IEnumerable<TestEntity>>._))
			.Invokes((IEnumerable<object> entities) =>
			{
				removed = entities.Select(x => (TestEntity)x).ToImmutableArray();
			});

		_context.AddRemoveRange(
			initial,
			update,
			key2 => new TestEntity(key1, key2)
		);

		A.CallTo(() => _context.AddRange(A<IEnumerable<object>>._)).MustHaveHappenedOnceExactly();
		A.CallTo(() => _context.AddRange(A<object[]>._)).MustNotHaveHappened();
		A.CallTo(() => _context.RemoveRange(A<IEnumerable<object>>._)).MustHaveHappenedOnceExactly();
		A.CallTo(() => _context.RemoveRange(A<object[]>._)).MustNotHaveHappened();

		added.Should().NotBeNull();
		added.Should().HaveSameCount(addedEntities);
		removed.Should().NotBeNull();
		removed.Should().HaveSameCount(removedEntities);

		added!.OrderBy(x => x.EntityKey2).Should().ContainInOrder(addedEntities);
		removed!.OrderBy(x => x.EntityKey2).Should().ContainInOrder(removedEntities);
	}

	public record TestEntity(Guid entityKey1, Guid EntityKey2);

	#endregion

	#region AddUpdateRemoveRange

	[Theory]
	[InlineData(
		new[] { "1e237b5f-f486-4583-b121-0885593c8242", "df27919c-30c6-4ee7-8356-7d9fef01e2a9", "61d8bfa5-242a-4c75-9115-84db9baba701", "f1166064-2457-458e-b8f9-32a481f34785", "7dda58bd-cfe5-4f0f-8070-ea57a8371c78", "10ed72c8-492b-4569-9448-8f438cfc6b55" }, // initialKeys
		new[] { "1e237b5f-f486-4583-b121-0885593c8242", "7f11d9a9-5ab3-4691-8a11-6250e72aa31d", "61d8bfa5-242a-4c75-9115-84db9baba701", "f1166064-2457-458e-b8f9-32a481f34785", "6f00c68f-85ca-47ba-b8ba-8fe8aa58b53d", "10ed72c8-492b-4569-9448-8f438cfc6b55" }, // updateKeys
		new[] { "c99f59dc-b41c-4dd2-ba17-4a260ea79666", "0565ab3b-a3a3-44ee-b1e1-45795bcaffdc", "178303ea-9ef6-42fc-85b2-f51c19b8a024", "55fbafdc-772f-421c-b8f3-9bee5c6fd1a8", "3e455974-5636-4319-a4f2-319886f34a7f", "20848b0b-d3dd-4888-9dee-af4f158818fe" }, // initialValues
		new[] { "c99f59dc-b41c-4dd2-ba17-4a260ea79666", "b1cad394-2f69-4c93-bda8-a025154fb25d", "804be63e-8d34-4c57-8295-7a95a920bbf9", "55fbafdc-772f-421c-b8f3-9bee5c6fd1a8", "c08b1687-0483-4e00-a36c-0414eeaef11b", "75adcc39-62e9-4299-bb4a-18183665a32c" }, // updateValues
		new[] { "7f11d9a9-5ab3-4691-8a11-6250e72aa31d", "6f00c68f-85ca-47ba-b8ba-8fe8aa58b53d" }, // addedEntityKeys
		new[] { "b1cad394-2f69-4c93-bda8-a025154fb25d", "c08b1687-0483-4e00-a36c-0414eeaef11b" }, // addedEntityValues
		new[] { "61d8bfa5-242a-4c75-9115-84db9baba701", "10ed72c8-492b-4569-9448-8f438cfc6b55" }, // updatedEntityKeys
		new[] { "804be63e-8d34-4c57-8295-7a95a920bbf9", "75adcc39-62e9-4299-bb4a-18183665a32c" }, // updatedEntityValues
		new[] { "df27919c-30c6-4ee7-8356-7d9fef01e2a9", "7dda58bd-cfe5-4f0f-8070-ea57a8371c78" }  // removedEntityKeys
	)]

	public void AddAttachRemoveRangeSuccess(
		IEnumerable<string> initialKeys, IEnumerable<string> updateKeys,
		IEnumerable<string> initialValues, IEnumerable<string> updateValues,
		IEnumerable<string> addedEntityKeys, IEnumerable<string> addedEntityValues,
		IEnumerable<string> updatedEntityKeys, IEnumerable<string> updatedEntityValues,
		IEnumerable<string> removedEntityKeys)
	{
		var initialItems = initialKeys.Zip(initialValues).Select(x => ((Guid InitialKey, string InitialValue))(new Guid(x.First), x.Second)).ToImmutableArray();
		var updateItems = updateKeys.Zip(updateValues).Select(x => ((Guid UpdateKey, string UpdateValue))(new Guid(x.First), x.Second)).ToImmutableArray();
		var addedEntities = addedEntityKeys.Zip(addedEntityValues).Select(x => new TestValueEntity(new Guid(x.First), x.Second)).ToImmutableArray();
		var updatedEntities = updatedEntityKeys.Zip(updatedEntityValues).Select(x => new TestValueEntity(new Guid(x.First), x.Second)).ToImmutableArray();
		var removedEntityIds = removedEntityKeys.Select(x => new Guid(x)).ToImmutableArray();

		IEnumerable<TestValueEntity>? added = null;
		IEnumerable<TestValueEntity>? updated = null;
		IEnumerable<TestValueEntity>? removed = null;

		A.CallTo(() => _context.AddRange(A<IEnumerable<TestValueEntity>>._))
			.Invokes((IEnumerable<object> entities) =>
			{
				added = entities.Select(x => (TestValueEntity)x).ToImmutableArray();
			});
		A.CallTo(() => _context.AttachRange(A<IEnumerable<TestValueEntity>>._))
			.Invokes((IEnumerable<object> entities) =>
			{
				updated = entities.Select(x => (TestValueEntity)x).ToImmutableArray();
			});
		A.CallTo(() => _context.RemoveRange(A<IEnumerable<TestValueEntity>>._))
			.Invokes((IEnumerable<object> entities) =>
			{
				removed = entities.Select(x => (TestValueEntity)x).ToImmutableArray();
			});

		_context.AddAttachRemoveRange(
			initialItems,
			updateItems,
			initialItem => initialItem.InitialKey,
			updateItem => updateItem.UpdateKey,
			key => new TestValueEntity(key, null!),
			(initialItem, updateItem) => initialItem.InitialValue == updateItem.UpdateValue,
			(entity, initialItem) => entity.EntityValue = initialItem.InitialValue,
			(entity, updateItem) => entity.EntityValue = updateItem.UpdateValue
		);

		A.CallTo(() => _context.AddRange(A<IEnumerable<object>>._)).MustHaveHappenedOnceExactly();
		A.CallTo(() => _context.AddRange(A<object[]>._)).MustNotHaveHappened();
		A.CallTo(() => _context.AttachRange(A<IEnumerable<object>>._)).MustHaveHappenedOnceExactly();
		A.CallTo(() => _context.AttachRange(A<object[]>._)).MustNotHaveHappened();
		A.CallTo(() => _context.RemoveRange(A<IEnumerable<object>>._)).MustHaveHappenedOnceExactly();
		A.CallTo(() => _context.RemoveRange(A<object[]>._)).MustNotHaveHappened();

		added.Should().NotBeNull()
			.And.HaveSameCount(addedEntities)
			.And.Contain(addedEntities);
		updated.Should().NotBeNull()
			.And.HaveSameCount(updatedEntities)
			.And.Contain(updatedEntities);
		removed.Should().NotBeNull();
		removed!.Select(x => x.EntityKey)
			.Should().HaveSameCount(removedEntityIds)
			.And.Contain(removedEntityIds);
	}

	public record TestValueEntity
	{
		public TestValueEntity(Guid entityKey, string entityValue)
		{
			EntityKey = entityKey;
			EntityValue = entityValue;
		}

		public Guid EntityKey { get; }
		public string EntityValue { get; set; }
	}

	#endregion
}
