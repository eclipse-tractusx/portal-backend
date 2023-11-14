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

using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;

[Serializable]
public abstract class DetailException : Exception
{
    private static readonly Regex _templateMatcherExpression = new Regex(@"\{(\w+)\}", RegexOptions.None, TimeSpan.FromSeconds(1)); // to replace any text surrounded by { and }
    private enum NoDetailsErrorType
    {
        NONE = 0
    }

    protected DetailException() : base() { }
    protected DetailException(string message) : base(message) { }
    protected DetailException(string message, Exception inner) : base(message, inner) { }
    protected DetailException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    protected DetailException(
        Type errorType, int errorCode, IEnumerable<ErrorParameter>? parameters, Exception? inner) : base(Enum.GetName(errorType, errorCode), inner)
    {
        ErrorType = errorType;
        ErrorCode = errorCode;
        Parameters = parameters ?? Enumerable.Empty<ErrorParameter>();
    }

    protected static int ValueOf<T>(T error) where T : Enum => (int)Convert.ChangeType(error, TypeCode.Int32);

    public Type ErrorType { get; } = typeof(NoDetailsErrorType);
    public int ErrorCode { get; } = (int)NoDetailsErrorType.NONE;
    public IEnumerable<ErrorParameter> Parameters { get; } = Enumerable.Empty<ErrorParameter>();
    public bool HasDetails { get => ErrorType != typeof(NoDetailsErrorType); }

    public string GetErrorMessage(IErrorMessageService messageService) =>
        _templateMatcherExpression.Replace(
            messageService.GetMessage(ErrorType, ErrorCode),
            m => Parameters.SingleOrDefault(x => x.Name == m.Groups[1].Value)?.Value ?? "null");

    public IEnumerable<ErrorDetails> GetErrorDetails(IErrorMessageService messageService) =>
        GetDetailExceptions().Select(x =>
            new ErrorDetails(
                x.Message,
                x.ErrorType.Name,
                messageService.GetMessage(x.ErrorType, x.ErrorCode),
                x.Parameters));

    private IEnumerable<DetailException> GetDetailExceptions()
    {
        yield return this;
        var inner = InnerException;
        while (inner is not null)
        {
            if (inner is DetailException detail && detail.ErrorType != typeof(NoDetailsErrorType))
                yield return detail;
            inner = inner.InnerException;
        }
    }
}
