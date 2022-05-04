using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Tests.Shared
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
