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

using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions
{
    public static class FakeDbSetExtensions
    {
        public static DbSet<T> AsFakeDbSet<T>(this IEnumerable<T> items) where T : class
        {
            // https://github.com/pushrbx/EntityFrameworkCore.Testing.FakeItEasy/blob/master/src/Microsoft.EntityFrameworkCore.Testing.FakeItEasy/Aef.cs
            var fakeDbSet = A.Fake<DbSet<T>>(o => o.Implements(typeof(IAsyncEnumerable<T>)).Implements(typeof(IQueryable<T>)));

            SetupQueryableDbSet(fakeDbSet, items);

            return fakeDbSet;
        }

        public static DbSet<T> AsFakeDbSet<T>(this ICollection<T> items) where T : class
        {
            // https://github.com/pushrbx/EntityFrameworkCore.Testing.FakeItEasy/blob/master/src/Microsoft.EntityFrameworkCore.Testing.FakeItEasy/Aef.cs
            var fakeDbSet = A.Fake<DbSet<T>>(o => o.Implements(typeof(IAsyncEnumerable<T>)).Implements(typeof(IQueryable<T>)));

            SetupQueryableDbSet(fakeDbSet, items);
            SetupCollectionDbSet(fakeDbSet, items);

            return fakeDbSet;
        }

        private static void SetupQueryableDbSet<T>(DbSet<T> fakeDbSet, IEnumerable<T> items) where T : class
        {
            var itemAsyncEnum = new AsyncEnumerableStub<T>(items);

            A.CallTo(() => fakeDbSet.AsQueryable()).ReturnsLazily(_ => itemAsyncEnum);
            A.CallTo(() => ((IQueryable<T>)fakeDbSet).Provider).ReturnsLazily(_ => itemAsyncEnum.AsQueryable().Provider);
            A.CallTo(() => ((IQueryable<T>)fakeDbSet).Expression).ReturnsLazily(_ => itemAsyncEnum.AsQueryable().Expression);
            A.CallTo(() => ((IQueryable<T>)fakeDbSet).ElementType).ReturnsLazily(_ => itemAsyncEnum.AsQueryable().ElementType);
            A.CallTo(() => ((IQueryable<T>)fakeDbSet).GetEnumerator()).ReturnsLazily(_ => itemAsyncEnum.AsQueryable().GetEnumerator());

            A.CallTo(() => fakeDbSet.AsAsyncEnumerable()).ReturnsLazily(_ => itemAsyncEnum);
            A.CallTo(() => ((IAsyncEnumerable<T>)fakeDbSet).GetAsyncEnumerator(A<CancellationToken>._)).ReturnsLazily(_ => itemAsyncEnum.GetAsyncEnumerator());
        }

        private static void SetupCollectionDbSet<T>(DbSet<T> fakeDbSet, ICollection<T> items) where T: class
        {
            A.CallTo(() => fakeDbSet.Add(A<T>._)).Invokes((T newItem) => items.Add(newItem));
            A.CallTo(() => fakeDbSet.AddAsync(A<T>._, A<CancellationToken>._)).Invokes((T newItem, CancellationToken token) => items.Add(newItem));
            A.CallTo(() => fakeDbSet.AddRange(A<IEnumerable<T>>._)).Invokes((IEnumerable<T> newItems) =>
            {
                foreach (var newItem in newItems)
                    items.Add(newItem);
            });
            A.CallTo(() => fakeDbSet.AddRangeAsync(A<IEnumerable<T>>._, A<CancellationToken>._)).Invokes((IEnumerable<T> newItems, CancellationToken token) =>
            {
                foreach (var newItem in newItems)
                    items.Add(newItem);
            });
            A.CallTo(() => fakeDbSet.Remove(A<T>._)).Invokes((T itemToRemove) => items.Remove(itemToRemove));
            A.CallTo(() => fakeDbSet.RemoveRange(A<IEnumerable<T>>._)).Invokes((IEnumerable<T> itemsToRemove) =>
            {
                foreach (var itemToRemove in itemsToRemove)
                    items.Remove(itemToRemove);
            });
        }
    }
}
