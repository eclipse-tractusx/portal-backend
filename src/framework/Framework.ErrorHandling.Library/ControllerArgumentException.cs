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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

/// <inheritdoc />
[Serializable]
public class ControllerArgumentException : Exception
{
    public ControllerArgumentException(string message) : base(message) { }

    public ControllerArgumentException(ArgumentException argumentException)
        : this(argumentException.Message)
    {
        ParamName = argumentException.ParamName;
    }

    public ControllerArgumentException(string message, string paramName)
        : base(String.Format("{0} (Parameter '{1}')", message, paramName))
    {
        ParamName = paramName;
    }

    public string? ParamName { get; }

    protected ControllerArgumentException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
