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

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

#pragma warning disable EF1001
public class CustomNpgsqlHistoryRepository(HistoryRepositoryDependencies dependencies) :
    NpgsqlHistoryRepository(dependencies), IHistoryRepository
{
    protected override string ExistsSql => $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = '{this.TableSchema}' AND table_name = '{this.TableName}');";

    public override bool Exists() =>
        this.Dependencies.DatabaseCreator.Exists() &&
        this.InterpretExistsResult(
            this.Dependencies.RawSqlCommandBuilder
                .Build(this.ExistsSql)
                .ExecuteScalar(new RelationalCommandParameterObject(this.Dependencies.Connection, null, null, this.Dependencies.CurrentContext.Context, this.Dependencies.CommandLogger, CommandSource.Migrations)));

    public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = new())
    {
        if (!await this.Dependencies.DatabaseCreator.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var existsCommand = await this.Dependencies.RawSqlCommandBuilder
            .Build(this.ExistsSql)
            .ExecuteScalarAsync(new RelationalCommandParameterObject(this.Dependencies.Connection, null, null, this.Dependencies.CurrentContext.Context, this.Dependencies.CommandLogger, CommandSource.Migrations), cancellationToken)
            .ConfigureAwait(false);
        return this.InterpretExistsResult(existsCommand);
    }

    bool IHistoryRepository.CreateIfNotExists()
    {
        if (Exists())
        {
            return true;
        }

        try
        {
            return Dependencies.MigrationCommandExecutor.ExecuteNonQuery(GetCreateIfNotExistsCommands(), Dependencies.Connection, new MigrationExecutionState(), commitTransaction: true) != 0;
        }
        catch (PostgresException e) when (e.SqlState is PostgresErrorCodes.UniqueViolation
                                              or PostgresErrorCodes.DuplicateTable
                                              or PostgresErrorCodes.DuplicateObject)
        {
            return false;
        }
    }

    async Task<bool> IHistoryRepository.CreateIfNotExistsAsync(CancellationToken cancellationToken)
    {
        if (await ExistsAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            return true;
        }

        try
        {
            return await Dependencies.MigrationCommandExecutor
                .ExecuteNonQueryAsync(
                    GetCreateIfNotExistsCommands(),
                    Dependencies.Connection,
                    new MigrationExecutionState(),
                    commitTransaction: true,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None) != 0;
        }
        catch (PostgresException e) when (e.SqlState is PostgresErrorCodes.UniqueViolation
                                              or PostgresErrorCodes.DuplicateTable
                                              or PostgresErrorCodes.DuplicateObject)
        {
            return false;
        }
    }

    private IReadOnlyList<MigrationCommand> GetCreateIfNotExistsCommands() =>
        Dependencies.MigrationsSqlGenerator.Generate(
            [
                new SqlOperation
                {
                    Sql = GetCreateIfNotExistsScript(),
                    SuppressTransaction = true
                }
            ]);
}
#pragma warning restore EF1001
