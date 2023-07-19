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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthenticationManagement;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class AuthenticationFlowsUpdater : IAuthenticationFlowsUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public AuthenticationFlowsUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public Task UpdateAuthenticationFlows(string keycloakInstanceName)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);

        var handler = new AuthenticationFlowHandler(keycloak, _seedData);

        return handler.UpdateAuthenticationFlows();
    }

    private class AuthenticationFlowHandler
    {
        private readonly string _realm;
        private readonly KeycloakClient _keycloak;
        private readonly ISeedDataHandler _seedData;

        public AuthenticationFlowHandler(KeycloakClient keycloak, ISeedDataHandler seedData)
        {
            _keycloak = keycloak;
            _seedData = seedData;
            _realm = seedData.Realm;
        }

        public async Task UpdateAuthenticationFlows()
        {
            var flows = (await _keycloak.GetAuthenticationFlowsAsync(_realm).ConfigureAwait(false));
            var seedFlows = _seedData.TopLevelCustomAuthenticationFlows;
            var topLevelCustomFlows = flows.Where(flow => !(flow.BuiltIn ?? false) && (flow.TopLevel ?? false));

            if (topLevelCustomFlows.ExceptBy(seedFlows.Select(x => x.Alias), x => x.Alias).IfAny(
                async deleteFlows =>
                {
                    foreach (var delete in deleteFlows)
                    {
                        if (delete.Id == null)
                            throw new ConflictException($"authenticationFlow.id is null {delete.Alias} {delete.Description}");
                        await _keycloak.DeleteAuthenticationFlowAsync(_realm, delete.Id).ConfigureAwait(false);
                    }
                },
                out var deleteFlowsTask
            ))
            {
                await deleteFlowsTask!.ConfigureAwait(false);
            }

            if (seedFlows.ExceptBy(topLevelCustomFlows.Select(x => x.Alias), x => x.Alias).IfAny(
                async addFlows =>
                {
                    foreach (var addFlow in addFlows)
                    {
                        if (addFlow.Alias == null)
                            throw new ConflictException($"authenticationFlow.Alias is null {addFlow.Id} {addFlow.Description}");
                        if (addFlow.BuiltIn ?? false)
                            throw new ConflictException($"authenticationFlow.buildIn is true. flow cannot be added: {addFlow.Alias}");
                        await _keycloak.CreateAuthenticationFlowAsync(_realm, CreateUpdateAuthenticationFlow(null, addFlow)).ConfigureAwait(false);
                        await UpdateAuthenticationFlowExecutions(addFlow.Alias);
                    }
                },
                out var addFlowsTasks
            ))
            {
                await addFlowsTasks!.ConfigureAwait(false);
            }

            if (topLevelCustomFlows.Join(
                    seedFlows,
                    x => x.Alias,
                    x => x.Alias,
                    (flow, seed) => (Flow: flow, Seed: seed)
                ).IfAny(
                async updateFlows =>
                {
                    foreach (var (flow, seed) in updateFlows)
                    {
                        if (flow.Id == null)
                            throw new ConflictException($"authenticationFlow.id is null {flow.Alias} {flow.Description}");
                        if (flow.Alias == null)
                            throw new ConflictException($"authenticationFlow.Alias is null {flow.Id} {flow.Description}");
                        if (!Compare(flow, seed))
                        {
                            await _keycloak.UpdateAuthenticationFlowAsync(_realm, flow.Id, CreateUpdateAuthenticationFlow(flow.Id, seed)).ConfigureAwait(false);
                        }
                        await UpdateAuthenticationFlowExecutions(flow.Alias);
                    }
                },
                out var updateFlowsTask
            ))
            {
                await updateFlowsTask!.ConfigureAwait(false);
            }
        }

        private async Task UpdateAuthenticationFlowExecutions(string alias)
        {
            var updateExecutions = _seedData.GetAuthenticationExecutions(alias);
            var executionNodes = ExecutionNode.Parse(await GetExecutions(alias).ConfigureAwait(false));

            if (CompareRecursive(executionNodes, updateExecutions))
            {
                return;
            }
            await DeleteExecutionsRecursive(executionNodes).ConfigureAwait(false);
            await AddExecutionsRecursive(alias, updateExecutions).ConfigureAwait(false);
            executionNodes = ExecutionNode.Parse(await GetExecutions(alias).ConfigureAwait(false));
            await UpdateExecutionsRecursive(alias, executionNodes, updateExecutions).ConfigureAwait(false);
        }

        private bool CompareRecursive(IEnumerable<ExecutionNode> executions, IEnumerable<AuthenticationExecutionModel> updateExecutions) =>
            executions.Count() == updateExecutions.Count() &&
            executions.Zip(
                updateExecutions.OrderBy(x => x.Priority),
                (execution, update) => (Node: execution, Update: update)).All(
                    x =>
                        (x.Node.Execution.AuthenticationFlow ?? false) == (x.Update.AuthenticatorFlow ?? false) &&
                        x.Node.Execution.AuthenticationFlow switch
                        {
                            true =>
                                CompareFlowExecutions(x.Node.Execution, x.Update) &&
                                CompareRecursive(x.Node.Children, _seedData.GetAuthenticationExecutions(x.Update.FlowAlias)),
                            _ =>
                                CompareExecutions(x.Node.Execution, x.Update)
                        });

        private async Task DeleteExecutionsRecursive(IEnumerable<ExecutionNode> executionNodes)
        {
            foreach (var executionNode in executionNodes)
            {
                if (executionNode.Execution.AuthenticationFlow ?? false)
                {
                    await DeleteExecutionsRecursive(executionNode.Children).ConfigureAwait(false);
                }
                await _keycloak.DeleteAuthenticationExecutionAsync(_realm, executionNode.Execution.Id ?? throw new ConflictException("authenticationFlow.Id is null")).ConfigureAwait(false);
            }
        }

        private async Task AddExecutionsRecursive(string? alias, IEnumerable<AuthenticationExecutionModel> seedExecutions)
        {
            foreach (var execution in seedExecutions)
            {
                await (execution.AuthenticatorFlow switch
                {
                    true => AddAuthenticationFlowExecutionRecursive(alias!, execution),
                    _ => _keycloak.AddAuthenticationFlowExecutionAsync(_realm, alias!, CreateDataWithProvider(execution))
                }).ConfigureAwait(false);
            }

            async Task AddAuthenticationFlowExecutionRecursive(string alias, AuthenticationExecutionModel execution)
            {
                await _keycloak.AddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(_realm, alias, CreateDataWithAliasTypeProviderDescription(execution)).ConfigureAwait(false);
                await AddExecutionsRecursive(execution.FlowAlias, _seedData.GetAuthenticationExecutions(execution.FlowAlias)).ConfigureAwait(false);
            }
        }

        private async Task UpdateExecutionsRecursive(string alias, IReadOnlyList<ExecutionNode> executionNodes, IEnumerable<AuthenticationExecutionModel> seedExecutions)
        {
            if (executionNodes.Count != seedExecutions.Count())
                throw new ArgumentException("number of elements in executionNodes doesn't match seedData");

            foreach (var (executionNode, update) in executionNodes.Zip(seedExecutions))
            {
                if ((executionNode.Execution.AuthenticationFlow ?? false) != (update.AuthenticatorFlow ?? false))
                    throw new ArgumentException("execution.AuthenticatorFlow doesn't match seedData");

                await (executionNode.Execution.AuthenticationFlow switch
                {
                    true => UpdateAuthenticationFlowExecutionRecursive(alias, executionNode, update),
                    _ => UpdateAuthenticationExecution(executionNode, update)
                }).ConfigureAwait(false);
            }

            async Task UpdateAuthenticationFlowExecutionRecursive(string alias, ExecutionNode executionNode, AuthenticationExecutionModel update)
            {
                if (!CompareFlowExecutions(executionNode.Execution, update))
                {
                    await _keycloak.UpdateAuthenticationFlowExecutionsAsync(
                        _realm,
                        alias,
                        new AuthenticationExecutionInfo
                        {
                            AuthenticationFlow = true,
                            Configurable = executionNode.Execution.Configurable,
                            Description = _seedData.GetAuthenticationFlow(update.FlowAlias).Description,
                            DisplayName = update.FlowAlias,
                            FlowId = executionNode.Execution.FlowId,
                            Id = executionNode.Execution.Id,
                            Index = executionNode.Execution.Index,
                            Level = executionNode.Execution.Level,
                            Requirement = update.Requirement,
                            RequirementChoices = executionNode.Execution.RequirementChoices
                        }).ConfigureAwait(false);
                }

                var seedExecutions = _seedData.GetAuthenticationExecutions(update.FlowAlias);

                await UpdateExecutionsRecursive(
                    update.FlowAlias!,
                    executionNode.Children,
                    seedExecutions).ConfigureAwait(false);
            }

            Task UpdateAuthenticationExecution(ExecutionNode executionNode, AuthenticationExecutionModel update)
            {
                if (!CompareExecutions(executionNode.Execution, update))
                {
                    return _keycloak.UpdateAuthenticationFlowExecutionsAsync(
                        _realm,
                        alias,
                        new AuthenticationExecutionInfo
                        {
                            Configurable = executionNode.Execution.Configurable,
                            DisplayName = executionNode.Execution.Description,
                            Id = executionNode.Execution.Id,
                            Index = executionNode.Execution.Index,
                            Level = executionNode.Execution.Level,
                            ProviderId = executionNode.Execution.ProviderId,
                            Requirement = update.Requirement,
                            RequirementChoices = executionNode.Execution.RequirementChoices
                        });
                }
                return Task.CompletedTask;
            }
        }

        private bool CompareFlowExecutions(AuthenticationFlowExecution execution, AuthenticationExecutionModel update) =>
            execution.Description == _seedData.GetAuthenticationFlow(update.FlowAlias).Description &&
            execution.DisplayName == update.FlowAlias &&
            execution.Requirement == update.Requirement;

        private static bool CompareExecutions(AuthenticationFlowExecution execution, AuthenticationExecutionModel update) =>
            execution.ProviderId == update.Authenticator &&
            execution.Requirement == update.Requirement;

        private Task<IEnumerable<AuthenticationFlowExecution>> GetExecutions(string alias) =>
            _keycloak.GetAuthenticationFlowExecutionsAsync(_realm, alias);

        private IDictionary<string, object> CreateDataWithAliasTypeProviderDescription(AuthenticationExecutionModel execution)
        {
            var seedFlow = _seedData.GetAuthenticationFlow(execution.FlowAlias);
            return new Dictionary<string, object> {
                { "alias", execution.FlowAlias ?? throw new ConflictException($"authenticationExecution.FlowAlias is null: {seedFlow.Alias}")},
                { "description", seedFlow.Description ?? throw new ConflictException($"authenticationFlow.ProviderId is null: {seedFlow.Alias}")},
                { "provider", "registration-page-form" },
                { "type", seedFlow.ProviderId ?? throw new ConflictException($"authenticationFlow.ProviderId is null: {seedFlow.Alias}")}
            };
        }

        private static IDictionary<string, object> CreateDataWithProvider(AuthenticationExecutionModel execution) =>
            new Dictionary<string, object>
            {
                { "provider", execution.Authenticator ?? throw new ConflictException("authenticationExecution.Authenticator is null")}
            };

        private class ExecutionNode
        {
            private readonly AuthenticationFlowExecution _execution;
            private readonly IReadOnlyList<ExecutionNode>? _children;

            private ExecutionNode(AuthenticationFlowExecution execution, IReadOnlyList<ExecutionNode>? children = null)
            {
                _execution = execution;
                _children = children;
            }

            public AuthenticationFlowExecution Execution
            {
                get => _execution;
            }

            public IReadOnlyList<ExecutionNode> Children
            {
                get => _children ?? throw new InvalidOperationException($"execution is not a flow: {_execution.DisplayName}");
            }

            public static IReadOnlyList<ExecutionNode> Parse(IEnumerable<AuthenticationFlowExecution> executions)
            {
                return ParseInternal(executions.GetHasNextEnumerator(), 0).ToImmutableList();
            }

            private static IEnumerable<ExecutionNode> ParseInternal(IHasNextEnumerator<AuthenticationFlowExecution> executions, int level)
            {
                while (executions.HasNext)
                {
                    var execution = executions.Current;
                    if (execution.Level < level)
                    {
                        yield break;
                    }

                    if (execution.Level > level)
                    {
                        throw new ConflictException($"unexpected raise of level to {executions.Current.Level}, {executions.Current.DisplayName}");
                    }

                    executions.Advance();
                    yield return execution.AuthenticationFlow switch
                    {
                        true => new ExecutionNode(execution, ParseInternal(executions, level + 1).ToImmutableList()),
                        _ => new ExecutionNode(execution)
                    };
                }
            }
        }
    }

    private static AuthenticationFlow CreateUpdateAuthenticationFlow(string? id, AuthenticationFlowModel update) => new AuthenticationFlow
    {
        Id = id,
        Alias = update.Alias,
        BuiltIn = update.BuiltIn,
        Description = update.Description,
        ProviderId = update.ProviderId,
        TopLevel = update.TopLevel
    };

    private static bool Compare(AuthenticationFlow flow, AuthenticationFlowModel update) =>
        flow.BuiltIn == update.BuiltIn &&
        flow.Description == update.Description &&
        flow.ProviderId == update.ProviderId &&
        flow.TopLevel == update.TopLevel;
}
