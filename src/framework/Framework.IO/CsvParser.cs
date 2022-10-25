/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using System.Text;

namespace Org.CatenaX.Ng.Portal.Backend.Framework.IO;

public static class CsvParser
{
    private static readonly string DefaultDocumentParameterName = "document";

    public static void ValidateContentTypeTextCSV(string contentType)
    {
        if (!contentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException("Only contentType text/csv files are allowed.");
        }
    }

    public static void ValidateCsvHeaders(string firstLine, IEnumerable<string> csvHeaders, string? documentParameterName = null)
    {
        var headers = firstLine.Split(",").GetEnumerator();
        foreach (var csvHeader in csvHeaders)
        {
            if (!headers.MoveNext())
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got ''", documentParameterName ?? DefaultDocumentParameterName);
            }
            if ((string)headers.Current != csvHeader)
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got '{headers.Current}'", documentParameterName ?? DefaultDocumentParameterName);
            }
        }
        if (headers.MoveNext())
        {
            throw new ControllerArgumentException($"unexpected header '{headers.Current}'", documentParameterName ?? DefaultDocumentParameterName);
        }
    }

    public static string NextStringItemIsNotNull(IEnumerator<string> items, object itemName, string? documentParameterName = null)
    {
        if(!items.MoveNext())
        {
            throw new ControllerArgumentException($"value for {itemName} type string expected", documentParameterName ?? DefaultDocumentParameterName);
        }
        return items.Current;
    }

    public static string NextStringItemIsNotNullOrWhiteSpace(IEnumerator<string> items, object itemName, string? documentParameterName = null)
    {
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {itemName} type string expected", documentParameterName ?? DefaultDocumentParameterName);
        }
        return items.Current;
    }

    public static IEnumerable<string> TrailingStringItemsNotNullOrWhiteSpace(IEnumerator<string> items, object itemName, string? documentParameterName = null)
    {
        while (items.MoveNext())
        {
            if(string.IsNullOrWhiteSpace(items.Current))
            {
                throw new ControllerArgumentException($"value for {itemName} type string expected", documentParameterName ?? DefaultDocumentParameterName);
            }
            yield return items.Current;
        }
    }

    public static async ValueTask<(int Processed, int Lines, IEnumerable<(int Line, Exception Error)> Errors)> ProcessCsvAsync<TLineType>(
        Stream stream,
        Action<string>? validateHeaderLine,
        Func<string,ValueTask<TLineType>> parseLineAsync,
        Func<IAsyncEnumerable<TLineType>,IAsyncEnumerable<(bool Processed, Exception? Error)>> processLines,
        CancellationToken cancellationToken)
    {
        var reader = new StreamReader(new CancellableStream(stream, cancellationToken), Encoding.UTF8);

        int numProcessed = 0;
        var errors = new List<(int Line, Exception Error)>();
        int numLines = 0;

        if (validateHeaderLine != null)
        {
            await ValidateFirstLineAsync(reader, validateHeaderLine).ConfigureAwait(false);
        }

        try
        {
            await foreach (var (processed,error) in processLines(ParseCsvLinesAsync(reader, parseLineAsync, error => { numLines++; errors.Add((numLines, error)); })))
            {
                if (error != null)
                {
                    errors.Add((numLines+1, error));
                }
                if (processed)
                {
                    numProcessed++;
                }
                numLines++;
            }
        }
        catch(Exception e)
        {
            numLines++;
            errors.Add((numLines, e));
        }
        return new (numProcessed, numLines, errors);
    }

    private static async IAsyncEnumerable<TLineType> ParseCsvLinesAsync<TLineType>(
        StreamReader reader,
        Func<string,ValueTask<TLineType>> parseLineAsync,
        Action<Exception> onError)
    {
        var nextLine = await reader.ReadLineAsync().ConfigureAwait(false);

        while (nextLine != null)
        {
            TLineType? result = default;
            try
            {
                result = await parseLineAsync(nextLine).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                onError(e);
                nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
                continue;
            }
            yield return result!; 
            nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
        }
    }

    private static async ValueTask ValidateFirstLineAsync(StreamReader reader, Action<string> validateFirstLine, string? documentParameterName = null)
    {
        var firstLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (firstLine == null)
        {
            throw new ControllerArgumentException("uploaded file contains no lines", documentParameterName ?? DefaultDocumentParameterName);
        }
        validateFirstLine(firstLine);
    }
}
