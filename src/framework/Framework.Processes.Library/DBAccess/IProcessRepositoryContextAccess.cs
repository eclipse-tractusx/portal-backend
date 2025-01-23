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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;

public interface IProcessRepositoryContextAccess<TProcess, TProcessType, TProcessStep, TProcessStepType, in TProcessTypeId, in TProcessStepTypeId> :
    IProcessDbContext<TProcess, TProcessType, TProcessStep, TProcessTypeId, TProcessStepTypeId>,
    IProcessCreation<TProcess, TProcessType, TProcessStep, TProcessStepType, TProcessTypeId, TProcessStepTypeId>
    where TProcess : class, IProcess<TProcessTypeId>, IProcessNavigation<TProcessType, TProcessStep, TProcessTypeId, TProcessStepTypeId>
    where TProcessStep : class, IProcessStep<TProcessStepTypeId>, IProcessStepNavigation<TProcess, TProcessStepType, TProcessTypeId, TProcessStepTypeId>
    where TProcessType : class, IProcessType<TProcessTypeId>, IProcessTypeNavigation<TProcess, TProcessTypeId>
    where TProcessStepType : class, IProcessStepType<TProcessStepTypeId>, IProcessStepTypeNavigation<TProcessStep, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible;
