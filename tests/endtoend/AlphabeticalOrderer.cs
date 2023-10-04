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

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class AlphabeticalOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
        IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
        testCases.OrderBy(testCase => testCase.TestMethod.Method.Name);
}

public class DisplayNameOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(
        IEnumerable<ITestCollection> testCollections) =>
        testCollections.OrderBy(collection => collection.DisplayName);
}
