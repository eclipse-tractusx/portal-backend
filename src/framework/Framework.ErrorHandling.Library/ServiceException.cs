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

using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

[Serializable]
public class ServiceException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public bool IsRecoverable { get; }

    public ServiceException(string message, bool isRecoverable = false) : base(message)
    {
        StatusCode = null;
        IsRecoverable = isRecoverable;
    }

    public ServiceException(string message, HttpStatusCode httpStatusCode, bool isRecoverable = false) : base(message)
    {
        StatusCode = httpStatusCode;
        IsRecoverable = isRecoverable;
    }

    public ServiceException(string message, System.Exception inner, bool isRecoverable = false) : base(message, inner)
    {
        StatusCode = null;
        IsRecoverable = isRecoverable;
    }

    public ServiceException(string message, System.Exception inner, HttpStatusCode httpStatusCode, bool isRecoverable = false) : base(message, inner)
    {
        StatusCode = httpStatusCode;
        IsRecoverable = isRecoverable;
    }

    protected ServiceException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
