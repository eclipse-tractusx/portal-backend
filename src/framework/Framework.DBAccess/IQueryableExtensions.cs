/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using System.Linq.Expressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

public static class IQueryableExtensions
{
    public static IQueryable<TQuery> JoinTuples<TQuery, TKey1, TKey2>(this IQueryable<TQuery> queryable, IEnumerable<ValueTuple<TKey1, TKey2>> criteria, Expression<Func<TQuery, TKey1>> queryableKey1Selector, Expression<Func<TQuery, TKey2>> queryableKey2Selector)
    {
        var paramSelect = Expression.Parameter(typeof(TQuery), "select");

        var where = Expression.Lambda<Func<TQuery, bool>>(
            criteria.Aggregate<(TKey1, TKey2), Expression>(
                Expression.Constant(false),
                (agg, crit) => Expression.OrElse(
                    agg,
                    Expression.AndAlso(
                        Expression.Equal(Expression.Invoke(queryableKey1Selector, paramSelect), Expression.Constant(crit.Item1)),
                        Expression.Equal(Expression.Invoke(queryableKey2Selector, paramSelect), Expression.Constant(crit.Item2))))),
            paramSelect);

        return queryable.Where(where);
    }
}
