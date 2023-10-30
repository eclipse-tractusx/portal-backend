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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;

[Serializable]
public class ConflictException : DetailException
{
    public ConflictException() : base() { }
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception inner) : base(message, inner) { }
    protected ConflictException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    protected ConflictException(Type errorType, int errorCode, IEnumerable<ErrorParameter>? parameters = null, Exception? inner = null) : base(errorType, errorCode, parameters, inner) { }

    public static ConflictException Create<T>(T error, IEnumerable<ErrorParameter>? parameters = null, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, inner);
    public static ConflictException Create<T>(T error, Exception inner) where T : Enum =>
        new(typeof(T), ValueOf(error), null, inner);
}
