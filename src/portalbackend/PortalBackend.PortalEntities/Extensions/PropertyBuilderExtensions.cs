/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;

public static class PropertyBuilderExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true
    };

    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
    {
        var converter = new ValueConverter<T, string>
        (
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<T>(v, Options) ?? new T()
        );

        var comparer = new ValueComparer<T?>
        (
            (l, r) => JsonSerializer.Serialize(l, Options) == JsonSerializer.Serialize(r, Options),
            v => v == null ? 0 : JsonSerializer.Serialize(v, Options).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, Options), Options)
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        propertyBuilder.HasColumnType("jsonb");

        return propertyBuilder;
    }

    public static PropertyBuilder<IReadOnlyDictionary<K, V>> HasJsonConversion<K, V>(this PropertyBuilder<IReadOnlyDictionary<K, V>> propertyBuilder) where K : class
    {
        var converter = new ValueConverter<IReadOnlyDictionary<K, V>, string>
        (
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<Dictionary<K, V>>(v, Options) ?? new Dictionary<K, V>()
        );

        var comparer = new ValueComparer<IReadOnlyDictionary<K, V>?>
        (
            (l, r) => JsonSerializer.Serialize(l, Options) == JsonSerializer.Serialize(r, Options),
            v => v == null ? 0 : JsonSerializer.Serialize(v, Options).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<K, V>>(JsonSerializer.Serialize(v, Options), Options)
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        propertyBuilder.HasColumnType("jsonb");

        return propertyBuilder;
    }
}
