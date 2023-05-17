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

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Attributes
{
    /// <summary>
    /// Attribute used for adding path metadata to a member.
    /// </summary>
    public class PathAttribute : Attribute
    {
        /// <summary>
        /// Path metadata of this attribute.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="path">Path to be attached to this attribute.</param>
        public PathAttribute(string path)
        {
            this.Path = path;
        }
    }
}
