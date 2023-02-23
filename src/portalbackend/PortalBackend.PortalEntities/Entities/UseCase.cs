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

using System.ComponentModel.DataAnnotations;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class UseCase : IBaseEntity
{
    private UseCase()
    {
        Name = null!;
        Shortname = null!;
        Agreements = new HashSet<Agreement>();
        Companies = new HashSet<Company>();
        Apps = new HashSet<Offer>();
    }
    
    public UseCase(Guid id, string name, string shortname) : this()
    {
        Id = id;
        Name = name;
        Shortname = shortname;
    }

    public Guid Id { get; private set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string Shortname { get; set; }

    // Navigation properties
    public virtual ICollection<Agreement> Agreements { get; private set; }
    public virtual ICollection<Company> Companies { get; private set; }
    public virtual ICollection<Offer> Apps { get; private set; }
}
