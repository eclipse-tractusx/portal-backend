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

public class SimpleProcessRepositories<TProcessTypeId, TProcessStepTypeId, TProcessDbContext>(TProcessDbContext dbContext) :
    AbstractRepositories<TProcessDbContext>(dbContext)
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
    where TProcessDbContext : class, IProcessDbContext<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, ProcessType<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId>, ProcessStep<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, IDbContext
{
    protected override IReadOnlyDictionary<Type, Func<TProcessDbContext, object>> RepositoryTypes => ProcessRepositoryTypes;
    private static readonly IReadOnlyDictionary<Type, Func<TProcessDbContext, object>> ProcessRepositoryTypes = ImmutableDictionary.CreateRange(
    [
        CreateTypeEntry<IProcessStepRepository<TProcessTypeId, TProcessStepTypeId>>(context =>
            new ProcessStepRepository<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, ProcessType<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId>, ProcessStep<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, ProcessStepType<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>(
                new SimpleProcessRepositoryContextAccess<TProcessTypeId, TProcessStepTypeId, TProcessDbContext>(context)))
    ]);
}
