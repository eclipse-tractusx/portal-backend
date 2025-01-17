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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.DBAccess;

public class ProcessRepositories<TProcessTypeId, TProcessStepTypeId, TProcessDbContext>(TProcessDbContext dbContext) :
    Repositories(dbContext)
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
    where TProcessDbContext : class, IProcessDbContext<Process<TProcessTypeId, TProcessStepTypeId>, ProcessStep<TProcessTypeId, TProcessStepTypeId>, ProcessStepStatus<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, IDbContext
{
    private static KeyValuePair<Type, Func<TProcessDbContext, object>> CreateTypeEntry<T>(Func<TProcessDbContext, object> createFunc) => KeyValuePair.Create(typeof(T), createFunc);

    protected static readonly IReadOnlyDictionary<Type, Func<TProcessDbContext, object>> ProcessRepositoryTypes = ImmutableDictionary.CreateRange(
    [
        CreateTypeEntry<IProcessStepRepository<TProcessTypeId, TProcessStepTypeId>>(context =>
            new ProcessStepRepository<Process<TProcessTypeId, TProcessStepTypeId>, ProcessStep<TProcessTypeId, TProcessStepTypeId>, ProcessStepStatus<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>(
                new ProcessRepositoryContextAccess<TProcessTypeId, TProcessStepTypeId, TProcessDbContext>(context)))
    ]);

    public override RepositoryType GetInstance<RepositoryType>()
    {
        object? repository = default;

        if (ProcessRepositoryTypes.TryGetValue(typeof(RepositoryType), out var createFunc))
        {
            repository = createFunc(dbContext);
        }

        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}", nameof(RepositoryType)));
    }
}
