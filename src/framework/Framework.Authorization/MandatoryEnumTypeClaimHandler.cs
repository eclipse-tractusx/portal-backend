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

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Authorization
{
    public class MandatoryEnumTypeClaimRequirement : IAuthorizationRequirement
    {
        private readonly string _claim;
        private readonly Type _enumType;
        private readonly int _value;
        public MandatoryEnumTypeClaimRequirement(string claim, object value)
        {
            _claim = claim;
            var type = value.GetType();
            if (type is null || !type.IsEnum)
            {
                throw new ArgumentException($"{value} is not an enum type");
            }
            _enumType = type;
            _value = (int)value;
        }
        public bool IsSuccess(IEnumerable<Claim> claims)
        {
            var claimValue = claims.FirstOrDefault(x => x.Type == _claim)?.Value;
            if (string.IsNullOrWhiteSpace(claimValue) ||
                !Enum.TryParse(_enumType, claimValue, out var enumValue) ||
                enumValue == null ||
                Array.BinarySearch(_enumType.GetEnumValues(), enumValue) < 0)
            {
                return false;
            }
            return _value == (int)enumValue;
        }
    }

    public class MandatoryEnumTypeClaimHandler : AuthorizationHandler<MandatoryEnumTypeClaimRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryEnumTypeClaimRequirement requirement)
        {
            if (requirement.IsSuccess(context.User.Claims))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}
