/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CatenaX.NetworkServices.Keycloak.Library.Common.Converters;

public abstract class JsonEnumConverter<TEnum> : JsonConverter
    where TEnum : struct, IConvertible
{
    protected abstract string EntityString { get; }

    protected abstract string ConvertToString(TEnum value);

    protected abstract TEnum ConvertFromString(string s);

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var actualValue = (TEnum)value;
        writer.WriteValue(ConvertToString(actualValue));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            var items = new List<TEnum>();
            var array = JArray.Load(reader);
            items.AddRange(array.Select(x => ConvertFromString(x.ToString())));

            return items;
        }

        string s = (string)reader.Value;
        return ConvertFromString(s);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }
}
