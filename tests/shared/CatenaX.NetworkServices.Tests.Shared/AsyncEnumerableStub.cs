using System.Linq.Expressions;

namespace CatenaX.NetworkServices.Tests.Shared
{
    public class AsyncEnumerableStub<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public AsyncEnumerableStub(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public AsyncEnumerableStub(Expression expression)
            : base(expression)
        { }

        IQueryProvider IQueryable.Provider => new AsyncQueryProviderStub<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumeratorStub<T>(this.AsEnumerable().GetEnumerator());
        }
    }
}
