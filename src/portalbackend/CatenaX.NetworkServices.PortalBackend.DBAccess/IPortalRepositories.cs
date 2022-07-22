using Microsoft.EntityFrameworkCore.Storage;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public interface IPortalRepositories
{
    /// <summary>
    /// Attaches the given Entity to the database
    /// </summary>
    /// <param name="entity">the entity that should be attached to the database</param>
    /// <typeparam name="TEntity">Type of the entity</typeparam>
    /// <returns>Returns the attached entity</returns>
    TEntity Attach<TEntity>(TEntity entity)
        where TEntity : class;

    /// <summary>
    /// Removes the given entity from the database
    /// </summary>
    /// <param name="entity">the entity that should be removed to the database</param>
    /// <typeparam name="TEntity">Type of the entity</typeparam>
    /// <returns>Returns the attached entity</returns>
    TEntity Remove<TEntity>(TEntity entity)
        where TEntity : class;

    public T GetInstance<T>();

    public Task<int> SaveAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
