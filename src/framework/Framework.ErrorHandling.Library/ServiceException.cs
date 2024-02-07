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

using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

[Serializable]
public class ServiceException : DetailException
{
    public HttpStatusCode? StatusCode { get; }
    public bool IsRecoverable { get; }

    public ServiceException() : base() { }

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

    public ServiceException(string message, Exception inner, bool isRecoverable = false) : base(message, inner)
    {
        StatusCode = null;
        IsRecoverable = isRecoverable;
    }

    public ServiceException(string message, Exception inner, HttpStatusCode httpStatusCode, bool isRecoverable = false) : base(message, inner)
    {
        StatusCode = httpStatusCode;
        IsRecoverable = isRecoverable;
    }

    protected ServiceException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    protected ServiceException(Type errorType, int errorCode, IEnumerable<ErrorParameter>? parameters = null, HttpStatusCode? httpStatusCode = null, bool isRecoverable = false, Exception? inner = null) : base(errorType, errorCode, parameters, inner)
    {
        StatusCode = httpStatusCode;
        IsRecoverable = isRecoverable;
    }

    public static ServiceException Create<T>(T error, IEnumerable<ErrorParameter>? parameters = null, HttpStatusCode? httpStatusCode = null, bool isRecoverable = false, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, httpStatusCode, isRecoverable, inner);
    public static ServiceException Create<T>(T error, HttpStatusCode httpStatusCode, bool isRecoverable = false, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), null, httpStatusCode, isRecoverable, inner);
    public static ServiceException Create<T>(T error, bool isRecoverable = false, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), null, null, isRecoverable, inner);
    public static ServiceException Create<T>(T error, IEnumerable<ErrorParameter> parameters, bool isRecoverable, Exception? inner = null) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, null, isRecoverable, inner);
    public static ServiceException Create<T>(T error, Exception inner) where T : Enum =>
        new(typeof(T), ValueOf(error), null, null, false, inner);
    public static ServiceException Create<T>(T error, IEnumerable<ErrorParameter> parameters, Exception inner) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, null, false, inner);
    public static ServiceException Create<T>(T error, IEnumerable<ErrorParameter> parameters, HttpStatusCode httpStatusCode, Exception inner) where T : Enum =>
        new(typeof(T), ValueOf(error), parameters, httpStatusCode, false, inner);
}
