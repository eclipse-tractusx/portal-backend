/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;

public class AdministrationDocumentErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AdministrationDocumentErrors.DOCUMENT_NOT_DOC_NOT_EXIST, "Document {documentId} does not exist"),
        new((int)AdministrationDocumentErrors.DOCUMENT_FORBIDDEN_USER_NOT_ALLOW_ACCESS_DOC, "User is not allowed to access the document"),
        new((int)AdministrationDocumentErrors.DOCUMENT_UNEXPECT_DOC_CONTENT_NOT_NULL, "documentContent should never be null here"),
        new((int)AdministrationDocumentErrors.DOCUMENT_NOT_SELFDESP_DOC_NOT_EXIST, "Self description document {documentId} does not exist"),
        new((int)AdministrationDocumentErrors.DOCUMENT_FORBIDDEN_USER_NOT_ALLOW_DEL_DOC, "User is not allowed to delete this document"),
        new((int)AdministrationDocumentErrors.DOCUMENT_ARGUMENT_INCORR_DOC_STATUS, "Incorrect document status"),
        new((int)AdministrationDocumentErrors.DOCUMENT_FORBIDDEN_ENDPOINT_ALLOW_USE_IN_DEV_ENV, "Endpoint can only be used on dev environment")
    ]);

    public Type Type { get => typeof(AdministrationDocumentErrors); }

    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationDocumentErrors
{
    DOCUMENT_NOT_DOC_NOT_EXIST,
    DOCUMENT_FORBIDDEN_USER_NOT_ALLOW_ACCESS_DOC,
    DOCUMENT_UNEXPECT_DOC_CONTENT_NOT_NULL,
    DOCUMENT_NOT_SELFDESP_DOC_NOT_EXIST,
    DOCUMENT_FORBIDDEN_USER_NOT_ALLOW_DEL_DOC,
    DOCUMENT_ARGUMENT_INCORR_DOC_STATUS,
    DOCUMENT_FORBIDDEN_ENDPOINT_ALLOW_USE_IN_DEV_ENV
}
