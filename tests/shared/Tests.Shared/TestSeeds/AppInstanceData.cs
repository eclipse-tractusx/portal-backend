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

using System.Collections.Immutable;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

public static class AppInstanceData
{
    public static readonly ImmutableList<AppInstance> AppInstances = ImmutableList.Create(
        new AppInstance(new Guid("89FF0C72-052F-4B1D-B5D5-89F3D61BA0B1"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("0c9051d0-d032-11ec-9d64-0242ac120002")), 
        new AppInstance(new Guid("B87F5778-928B-4375-B653-0D6F28E2A1C1"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("f032a034-d035-11ec-9d64-0242ac120002")), 
        new AppInstance(new Guid("C398F1E9-92A2-4C76-89DC-062FBD7CA6F1"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("cf207afb-d213-4c33-becc-0cabeef174a7")),
        new AppInstance(new Guid("C398F1E9-92A2-4C76-89DC-062FBD7CA6F2"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), new Guid("cf207afb-d213-4c33-becc-0cabeef174a7"))
    );
}
