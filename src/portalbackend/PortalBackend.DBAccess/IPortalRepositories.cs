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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;

public interface IPortalRepositories
{
	/// <summary>
	/// Attaches the given Entity to the database
	/// </summary>
	/// <param name="entity">the entity that should be attached to the database</param>
	/// <typeparam name="TEntity">Type of the entity</typeparam>
	/// <returns>Returns the attached entity</returns>
	TEntity Attach<TEntity>(TEntity entity, Action<TEntity>? setOptionalParameters = null)
		where TEntity : class;

	void AttachRange<TEntity>(IEnumerable<TEntity> entities, Action<TEntity> setOptionalParameters) where TEntity : class;

	IEnumerable<TEntity> AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

	/// <summary>
	/// Removes the given entity from the database
	/// </summary>
	/// <param name="entity">the entity that should be removed to the database</param>
	/// <typeparam name="TEntity">Type of the entity</typeparam>
	/// <returns>Returns the attached entity</returns>
	TEntity Remove<TEntity>(TEntity entity)
		where TEntity : class;

	void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

	public T GetInstance<T>();

	public Task<int> SaveAsync();
	void Clear();
}
