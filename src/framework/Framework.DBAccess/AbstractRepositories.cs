/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

public abstract class AbstractRepositories<TDbContext>(TDbContext dbContext) : IRepositories
    where TDbContext : class, IDbContext
{
    protected static KeyValuePair<Type, Func<TDbContext, object>> CreateTypeEntry<T>(Func<TDbContext, object> createFunc) => KeyValuePair.Create(typeof(T), createFunc);
    protected abstract IReadOnlyDictionary<Type, Func<TDbContext, object>> RepositoryTypes { get; }

    public RepositoryType GetInstance<RepositoryType>()
    {
        object? repository = default;

        if (RepositoryTypes.TryGetValue(typeof(RepositoryType), out var createFunc))
        {
            repository = createFunc(dbContext);
        }

        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}", nameof(RepositoryType)));
    }

    /// <inheritdoc />
    public TEntity Attach<TEntity>(TEntity entity, Action<TEntity>? setOptionalParameters = null) where TEntity : class
    {
        var attachedEntity = dbContext.Attach(entity).Entity;
        setOptionalParameters?.Invoke(attachedEntity);

        return attachedEntity;
    }

    /// <inheritdoc />
    public TEntity Remove<TEntity>(TEntity entity) where TEntity : class
        => dbContext.Remove(entity).Entity;

    public Task<int> SaveAsync()
    {
        try
        {
            return dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("while processing a concurrent update was saved to the database (reason could also be data to be deleted is no longer existing)", e);
        }
    }

    public void Clear() => dbContext.ChangeTracker.Clear();
}
