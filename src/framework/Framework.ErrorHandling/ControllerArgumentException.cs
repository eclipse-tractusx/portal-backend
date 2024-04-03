/********************************************************************************
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
public class ControllerArgumentException : DetailException
{
    public ControllerArgumentException() : base() { }

    public ControllerArgumentException(string message) : base(message) { }

    public ControllerArgumentException(string message, string paramName)
        : base(string.Format("{0} (Parameter '{1}')", message, paramName))
    {
        ParamName = paramName;
    }

    public string? ParamName { get; }

    protected ControllerArgumentException(Type errorType, int errorCode, IEnumerable<ErrorParameter>? parameters = null, string? paramName = null, Exception? inner = null) : base(errorType, errorCode, parameters, inner)
    {
        ParamName = paramName;
    }

    public static ControllerArgumentException Create<T>(T error, IEnumerable<ErrorParameter>? parameters = null, string? paramName = null, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, paramName, inner);
    public static ControllerArgumentException Create<T>(T error, Exception inner) where T : Enum =>
        new(typeof(T), ValueOf(error), null, null, inner);
    public static ControllerArgumentException Create<T>(T error, IEnumerable<ErrorParameter> parameters, Exception inner) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, null, inner);
    public static ControllerArgumentException Create<T>(T error, string paramName, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), null, paramName, inner);
}
