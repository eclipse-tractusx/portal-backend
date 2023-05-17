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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

public static class ContextExtensions
{
    public static void AddRemoveRange<TEntity, TKey>(
        this DbContext context,
        IEnumerable<TKey> initialKeys,
        IEnumerable<TKey> modifyKeys,
        Func<TKey, TEntity> createSelector) where TEntity : class
    {
        context.AddRange(
            modifyKeys
                .Except(initialKeys)
                .Select(modifyKey => createSelector(modifyKey)));

        context.RemoveRange(
            initialKeys
                .Except(modifyKeys)
                .Select(initialKey => createSelector(initialKey)));
    }

    public static void AddRemoveRange<TModify, TEntity, TKey>(
        this DbContext context,
        IEnumerable<TKey> initialKeys,
        IEnumerable<TModify> modifyItems,
        Func<TModify, TKey> modifyKeySelector,
        Func<TKey, TEntity> createSelector,
        Action<TEntity, TModify> modify) where TEntity : class
    {
        context.AddRange(
            modifyItems
                .ExceptBy(
                    initialKeys,
                    modifyKeySelector)
                .Select(
                    modifyItem =>
                    {
                        var entity = createSelector(modifyKeySelector(modifyItem));
                        modify(entity, modifyItem);
                        return entity;
                    }));

        context.RemoveRange(
            initialKeys
                .Except(modifyItems.Select(modifyKeySelector))
                .Select(initialKey => createSelector(initialKey)));
    }

    public static IEnumerable<TEntity> AddAttachRemoveRange<TInitial, TModify, TEntity, TKey>(
        this DbContext context,
        IEnumerable<TInitial> initialItems,
        IEnumerable<TModify> modifyItems,
        Func<TInitial, TKey> initialKeySelector,
        Func<TModify, TKey> modifyKeySelector,
        Func<TKey, TEntity> createSelector,
        Func<TInitial, TModify, bool> equalsPredicate,
        Action<TEntity, TInitial> initialize,
        Action<TEntity, TModify> modify) where TEntity : class =>

        AddAttachRemoveRange(
            context,
            initialItems,
            modifyItems,
            initialKeySelector,
            modifyKeySelector,
            (TInitial initial) =>
            {
                var entity = createSelector(initialKeySelector(initial));
                initialize(entity, initial);
                return entity;
            },
            (TModify modify) => createSelector(modifyKeySelector(modify)),
            equalsPredicate,
            modify);

    public static IEnumerable<TEntity> AddAttachRemoveRange<TInitial, TModify, TEntity, TKey>(
        this DbContext context,
        IEnumerable<TInitial> initialItems,
        IEnumerable<TModify> modifyItems,
        Func<TInitial, TKey> initialKeySelector,
        Func<TModify, TKey> modifyKeySelector,
        Func<TInitial, TEntity> initialCreateSelector,
        Func<TModify, TEntity> modifyCreateSelector,
        Func<TInitial, TModify, bool> equalsPredicate,
        Action<TEntity, TModify> modify) where TEntity : class
    {
        context.RemoveRange(
            initialItems
                .ExceptBy(modifyItems.Select(modifyKeySelector), initialKeySelector)
                .Select(initial => initialCreateSelector(initial)));

        return AddAttachRange(
            context,
            initialItems,
            modifyItems,
            initialKeySelector,
            modifyKeySelector,
            initialCreateSelector,
            modifyCreateSelector,
            equalsPredicate,
            modify);
    }

    public static IEnumerable<TEntity> AddAttachRange<TInitial, TModify, TEntity, TKey>(
        this DbContext context,
        IEnumerable<TInitial> initialItems,
        IEnumerable<TModify> modifyItems,
        Func<TInitial, TKey> initialKeySelector,
        Func<TModify, TKey> modifyKeySelector,
        Func<TKey, TEntity> createSelector,
        Func<TInitial, TModify, bool> equalsPredicate,
        Action<TEntity, TInitial> initialize,
        Action<TEntity, TModify> modify) where TEntity : class =>

        AddAttachRange(
            context,
            initialItems,
            modifyItems,
            initialKeySelector,
            modifyKeySelector,
            (TInitial initial) =>
            {
                var entity = createSelector(initialKeySelector(initial));
                initialize(entity, initial);
                return entity;
            },
            (TModify modify) => createSelector(modifyKeySelector(modify)),
            equalsPredicate,
            modify);

    public static IEnumerable<TEntity> AddAttachRange<TInitial, TModify, TEntity, TKey>(
        this DbContext context,
        IEnumerable<TInitial> initialItems,
        IEnumerable<TModify> modifyItems,
        Func<TInitial, TKey> initialKeySelector,
        Func<TModify, TKey> modifyKeySelector,
        Func<TInitial, TEntity> initialCreateSelector,
        Func<TModify, TEntity> modifyCreateSelector,
        Func<TInitial, TModify, bool> equalsPredicate,
        Action<TEntity, TModify> modify) where TEntity : class
    {
        var add = modifyItems
            .ExceptBy(
                initialItems.Select(initialKeySelector),
                modifyKeySelector)
            .Select(
                modifyItem =>
                {
                    var entity = modifyCreateSelector(modifyItem);
                    modify(entity, modifyItem);
                    return entity;
                }).ToImmutableList();
        context.AddRange(add);

        var joined = initialItems
            .Join(
                modifyItems,
                initialKeySelector,
                modifyKeySelector,
                (initialItem, modifyItem) => (Entity: initialCreateSelector(initialItem), initialItem, modifyItem))
            .ToImmutableList();

        var update = joined.Where(x => !equalsPredicate(x.initialItem, x.modifyItem)).ToImmutableList();
        context.AttachRange(update.Select(x => x.Entity));
        update.ForEach(x => modify(x.Entity, x.modifyItem));

        return add.Concat(joined.Select(x => x.Entity));
    }
}
