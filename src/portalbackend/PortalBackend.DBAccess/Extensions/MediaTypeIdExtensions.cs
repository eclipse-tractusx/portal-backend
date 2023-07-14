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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Net.Mime;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;

public static class MediaTypeIdExtensions
{
    public static string MapToMediaType(this MediaTypeId mediaTypeId)
    {
        return mediaTypeId switch
        {
            MediaTypeId.JPEG => MediaTypeNames.Image.Jpeg,
            MediaTypeId.GIF => MediaTypeNames.Image.Gif,
            MediaTypeId.PNG => "image/png",
            MediaTypeId.SVG => "image/svg+xml",
            MediaTypeId.TIFF => MediaTypeNames.Image.Tiff,
            MediaTypeId.PDF => MediaTypeNames.Application.Pdf,
            MediaTypeId.JSON => MediaTypeNames.Application.Json,
            MediaTypeId.PEM => "application/x-pem-file",
            MediaTypeId.CA_CERT => "application/x-x509-ca-cert",
            MediaTypeId.PKX_CER => "application/pkix-cert",
            MediaTypeId.OCTET => MediaTypeNames.Application.Octet,
            _ => throw new ConflictException($"document mediatype {mediaTypeId} is not supported")
        };
    }

    public static MediaTypeId ParseMediaTypeId(this string mediaType)
    {
        return mediaType.ToLower() switch
        {
            MediaTypeNames.Image.Jpeg => MediaTypeId.JPEG,
            "image/png" => MediaTypeId.PNG,
            MediaTypeNames.Image.Gif => MediaTypeId.GIF,
            "image/svg+xml" => MediaTypeId.SVG,
            MediaTypeNames.Image.Tiff => MediaTypeId.TIFF,
            MediaTypeNames.Application.Pdf => MediaTypeId.PDF,
            MediaTypeNames.Application.Json => MediaTypeId.JSON,
            "application/x-pem-file" => MediaTypeId.PEM,
            "application/x-x509-ca-cert" => MediaTypeId.CA_CERT,
            "application/pkix-cert" => MediaTypeId.PKX_CER,
            MediaTypeNames.Application.Octet => MediaTypeId.OCTET,
            _ => throw new UnsupportedMediaTypeException($"mediaType '{mediaType}' is not supported")
        };
    }

    public static void CheckDocumentContentType(this MediaTypeId mediaTypeId, IEnumerable<MediaTypeId> validMediaTypes)
    {
        if (!validMediaTypes.Contains(mediaTypeId))
        {
            throw new UnsupportedMediaTypeException($"Document type not supported. File must match contentTypes :{string.Join(",", validMediaTypes.Select(x => x.MapToMediaType()))}");
        }
    }
}
