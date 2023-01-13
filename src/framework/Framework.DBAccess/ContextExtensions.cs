/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

using Microsoft.EntityFrameworkCore;

public static class ContextExtensions
{
    public static void AddAttachRemoveRange<TInitial,TModify,TEntity,TKey>(
        this DbContext context,
        IEnumerable<TInitial> initialItems,
        IEnumerable<TModify> modifyItems,
        Func<TInitial,TKey> initialKeySelector,
        Func<TModify,TKey> modifyKeySelector,
        Func<TKey,TEntity> createSelector,
        Func<TInitial,TModify,bool> equalsPredicate,        
        Action<TEntity,TInitial> initialize,
        Action<TEntity,TModify> modify) where TEntity : class
    {
        context.AddRange(
            modifyItems
                .ExceptBy(
                    initialItems.Select(initialKeySelector),
                    modifyKeySelector)
                .Select(
                    modifyItem => {
                        var entity = createSelector(modifyKeySelector(modifyItem));
                        modify(entity, modifyItem);
                        return entity;
                    }));

        context.RemoveRange(
            initialItems
                .ExceptBy(
                    modifyItems.Select(modifyKeySelector),
                    initialKeySelector)
                .Select(initialItem => createSelector(initialKeySelector(initialItem))));

        var joined = initialItems
            .Join(
                modifyItems,
                initialKeySelector,
                modifyKeySelector,
                (initialItem, modifyItem) => (initialItem, modifyItem))
            .Where(x => !equalsPredicate(x.initialItem, x.modifyItem))
            .Select(x => new {
                x.initialItem,
                x.modifyItem,
                entity = createSelector(initialKeySelector(x.initialItem))
            })
            .ToList();
        context.AttachRange(
            joined
                .Select(
                    x => {
                        initialize(x.entity, x.initialItem);
                        return(x.entity);
                    }));
        joined.ForEach(x => modify(x.entity, x.modifyItem));
    }
}
