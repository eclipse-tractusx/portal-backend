/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class AuthenticationFlowsUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IAuthenticationFlowsUpdater
{
    public Task UpdateAuthenticationFlows(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);

        var handler = new AuthenticationFlowHandler(keycloak, seedDataHandler);

        return handler.UpdateAuthenticationFlows(cancellationToken);
    }

    private sealed class AuthenticationFlowHandler(KeycloakClient keycloak, ISeedDataHandler seedDataHandler)
    {
        private readonly string _realm = seedDataHandler.Realm;

        public async Task UpdateAuthenticationFlows(CancellationToken cancellationToken)
        {
            var flows = await keycloak.GetAuthenticationFlowsAsync(_realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            var seedFlows = seedDataHandler.TopLevelCustomAuthenticationFlows;
            var topLevelCustomFlows = flows.Where(flow => !(flow.BuiltIn ?? false) && (flow.TopLevel ?? false));
            var seederConfiguration = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.AuthenticationFlows);
            var authFlowExecutionConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.AuthenticationFlowExecution);
            var authenticatorConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.AuthenticatorConfig);

            await DeleteRedundantAuthenticationFlows(topLevelCustomFlows, seedFlows, seederConfiguration, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await AddMissingAuthenticationFlows(topLevelCustomFlows, seedFlows, seederConfiguration, authenticatorConfig, authFlowExecutionConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await UpdateExistingAuthenticationFlows(topLevelCustomFlows, seedFlows, seederConfiguration, authenticatorConfig, authFlowExecutionConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        private async Task DeleteRedundantAuthenticationFlows(IEnumerable<AuthenticationFlow> topLevelCustomFlows, IEnumerable<AuthenticationFlowModel> seedFlows, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
        {
            foreach (var delete in topLevelCustomFlows
                         .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x.Alias))
                         .ExceptBy(seedFlows.Select(x => x.Alias), x => x.Alias))
            {
                if (delete.Id == null)
                    throw new ConflictException($"authenticationFlow.id is null {delete.Alias} {delete.Description}");
                await keycloak.DeleteAuthenticationFlowAsync(_realm, delete.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        private async Task AddMissingAuthenticationFlows(IEnumerable<AuthenticationFlow> topLevelCustomFlows, IEnumerable<AuthenticationFlowModel> seedFlows, KeycloakSeederConfigModel seederConfig, KeycloakSeederConfigModel authenticatorConfig, KeycloakSeederConfigModel authFlowExecutionConfig, CancellationToken cancellationToken)
        {
            foreach (var addFlow in seedFlows
                         .ExceptBy(topLevelCustomFlows.Select(x => x.Alias), x => x.Alias))
            {
                if (addFlow.Alias == null)
                    throw new ConflictException($"authenticationFlow.Alias is null {addFlow.Id} {addFlow.Description}");
                if (addFlow.BuiltIn ?? false)
                    throw new ConflictException($"authenticationFlow.buildIn is true. flow cannot be added: {addFlow.Alias}");
                if (seederConfig.ModificationAllowed(ModificationType.Create, addFlow.Alias))
                {
                    await keycloak.CreateAuthenticationFlowAsync(_realm, CreateUpdateAuthenticationFlow(null, addFlow), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                }

                await UpdateAuthenticationFlowExecutions(addFlow.Alias, authenticatorConfig, authFlowExecutionConfig, cancellationToken);
            }
        }

        private async Task UpdateExistingAuthenticationFlows(IEnumerable<AuthenticationFlow> topLevelCustomFlows, IEnumerable<AuthenticationFlowModel> seedFlows, KeycloakSeederConfigModel seederConfig, KeycloakSeederConfigModel authenticatorConfig, KeycloakSeederConfigModel authFlowExecutionConfig, CancellationToken cancellationToken)
        {
            foreach (var (flow, seed) in topLevelCustomFlows
                .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x.Alias))
                .Join(
                    seedFlows,
                    x => x.Alias,
                    x => x.Alias,
                    (flow, seed) => (Flow: flow, Seed: seed)))
            {
                if (flow.Id == null)
                    throw new ConflictException($"authenticationFlow.id is null {flow.Alias} {flow.Description}");
                if (flow.Alias == null)
                    throw new ConflictException($"authenticationFlow.Alias is null {flow.Id} {flow.Description}");
                if (!CompareAuthenticationFlow(flow, seed))
                {
                    await keycloak.UpdateAuthenticationFlowAsync(_realm, flow.Id, CreateUpdateAuthenticationFlow(flow.Id, seed), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                }

                await UpdateAuthenticationFlowExecutions(flow.Alias, authenticatorConfig, authFlowExecutionConfig, cancellationToken);
            }
        }

        private static AuthenticationFlow CreateUpdateAuthenticationFlow(string? id, AuthenticationFlowModel update) => new()
        {
            Id = id,
            Alias = update.Alias,
            BuiltIn = update.BuiltIn,
            Description = update.Description,
            ProviderId = update.ProviderId,
            TopLevel = update.TopLevel
        };

        private static bool CompareAuthenticationFlow(AuthenticationFlow flow, AuthenticationFlowModel update) =>
            flow.BuiltIn == update.BuiltIn &&
            flow.Description == update.Description &&
            flow.ProviderId == update.ProviderId &&
            flow.TopLevel == update.TopLevel;

        private async Task UpdateAuthenticationFlowExecutions(string alias, KeycloakSeederConfigModel authenticatorConfig, KeycloakSeederConfigModel authFlowExecutionConfig, CancellationToken cancellationToken)
        {
            var updateExecutions = seedDataHandler.GetAuthenticationExecutions(alias);
            var executionNodes = ExecutionNode.Parse(await GetExecutions(alias, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None));

            if (!CompareStructureRecursive(executionNodes, updateExecutions))
            {
                await DeleteExecutionsRecursive(executionNodes, authFlowExecutionConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                await AddExecutionsRecursive(alias, updateExecutions, authFlowExecutionConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                executionNodes = ExecutionNode.Parse(await GetExecutions(alias, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None));
            }

            await UpdateExecutionsRecursive(alias, executionNodes, updateExecutions, authenticatorConfig, authFlowExecutionConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        private bool CompareStructureRecursive(IReadOnlyList<ExecutionNode> executions, IEnumerable<AuthenticationExecutionModel> updateExecutions) =>
            executions.Count == updateExecutions.Count() &&
            executions.Zip(
                updateExecutions.OrderBy(x => x.Priority),
                (execution, update) => (Node: execution, Update: update)).All(
                    x =>
                        (x.Node.Execution.AuthenticationFlow ?? false) == (x.Update.AuthenticatorFlow ?? false) &&
                        (!(x.Node.Execution.AuthenticationFlow ?? false) || CompareStructureRecursive(x.Node.Children, seedDataHandler.GetAuthenticationExecutions(x.Update.FlowAlias))));

        private async Task DeleteExecutionsRecursive(IEnumerable<ExecutionNode> executionNodes, KeycloakSeederConfigModel authFlowExecutionConfig, CancellationToken cancellationToken)
        {
            foreach (var executionNode in executionNodes.Where(x => authFlowExecutionConfig.ModificationAllowed(ModificationType.Delete, x.Execution.Id)))
            {
                if (executionNode.Execution.AuthenticationFlow ?? false)
                {
                    await DeleteExecutionsRecursive(executionNode.Children, authFlowExecutionConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                }

                await keycloak.DeleteAuthenticationExecutionAsync(_realm, executionNode.Execution.Id ?? throw new ConflictException("authenticationFlow.Id is null"), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        private async Task AddExecutionsRecursive(string? alias, IEnumerable<AuthenticationExecutionModel> seedExecutions, KeycloakSeederConfigModel authFlowExecutionConfig, CancellationToken cancellationToken)
        {
            foreach (var execution in seedExecutions.Where(x => authFlowExecutionConfig.ModificationAllowed(ModificationType.Create, x.FlowAlias)))
            {
                await (execution.AuthenticatorFlow switch
                {
                    true => AddAuthenticationFlowExecutionRecursive(alias!, execution, cancellationToken),
                    _ => keycloak.AddAuthenticationFlowExecutionAsync(_realm, alias!, CreateDataWithProvider(execution), cancellationToken)
                }).ConfigureAwait(ConfigureAwaitOptions.None);
            }

            async Task AddAuthenticationFlowExecutionRecursive(string updateAlias, AuthenticationExecutionModel execution, CancellationToken ct)
            {
                await keycloak.AddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(_realm, updateAlias, CreateDataWithAliasTypeProviderDescription(execution), ct).ConfigureAwait(ConfigureAwaitOptions.None);
                await AddExecutionsRecursive(execution.FlowAlias, seedDataHandler.GetAuthenticationExecutions(execution.FlowAlias), authFlowExecutionConfig, ct).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        private async Task UpdateExecutionsRecursive(string alias, IReadOnlyCollection<ExecutionNode> executionNodes, IEnumerable<AuthenticationExecutionModel> seedExecutions, KeycloakSeederConfigModel authenticatorConfig, KeycloakSeederConfigModel authFlowExecutionConfig, CancellationToken cancellationToken)
        {
            if (executionNodes.Count != seedExecutions.Count())
                throw new ArgumentException("number of elements in executionNodes doesn't match seedData");

            foreach (var (executionNode, update) in executionNodes.Zip(seedExecutions).Where(x => authFlowExecutionConfig.ModificationAllowed(ModificationType.Update, x.First.Execution.Id)))
            {
                if ((executionNode.Execution.AuthenticationFlow ?? false) != (update.AuthenticatorFlow ?? false))
                    throw new ArgumentException("execution.AuthenticatorFlow doesn't match seedData");

                await (executionNode.Execution.AuthenticationFlow switch
                {
                    true => UpdateAuthenticationFlowExecutionRecursive(alias, executionNode, update, cancellationToken),
                    _ => UpdateAuthenticationExecution(executionNode, update, cancellationToken)
                }).ConfigureAwait(ConfigureAwaitOptions.None);
            }

            async Task UpdateAuthenticationFlowExecutionRecursive(string updateAlias, ExecutionNode executionNode, AuthenticationExecutionModel update, CancellationToken ct)
            {
                if (!CompareFlowExecutions(executionNode.Execution, update))
                {
                    await keycloak.UpdateAuthenticationFlowExecutionsAsync(
                        _realm,
                        updateAlias,
                        new AuthenticationExecutionInfo
                        {
                            Alias = update.FlowAlias,
                            AuthenticationFlow = true,
                            Configurable = executionNode.Execution.Configurable,
                            Description = seedDataHandler.GetAuthenticationFlow(update.FlowAlias).Description,
                            DisplayName = update.FlowAlias,
                            FlowId = executionNode.Execution.FlowId,
                            Id = executionNode.Execution.Id,
                            Index = executionNode.Execution.Index,
                            Level = executionNode.Execution.Level,
                            Requirement = update.Requirement,
                            RequirementChoices = executionNode.Execution.RequirementChoices
                        },
                        ct).ConfigureAwait(ConfigureAwaitOptions.None);
                }

                var executions = seedDataHandler.GetAuthenticationExecutions(update.FlowAlias);

                await UpdateExecutionsRecursive(
                    update.FlowAlias!,
                    executionNode.Children,
                    executions,
                    authenticatorConfig,
                    authFlowExecutionConfig,
                    ct).ConfigureAwait(ConfigureAwaitOptions.None);
            }

            async Task UpdateAuthenticationExecution(ExecutionNode executionNode, AuthenticationExecutionModel update, CancellationToken ct)
            {
                var (isEqual, config) = await CompareExecutions(executionNode.Execution, update, ct).ConfigureAwait(ConfigureAwaitOptions.None);
                if (!isEqual && authFlowExecutionConfig.ModificationAllowed(ModificationType.Update, executionNode.Execution.Id))
                {
                    await keycloak.UpdateAuthenticationFlowExecutionsAsync(
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
                        },
                        ct).ConfigureAwait(ConfigureAwaitOptions.None);

                    await UpdateAuthenticatorConfig(executionNode.Execution, update, config, authenticatorConfig, ct).ConfigureAwait(ConfigureAwaitOptions.None);
                }
            }
        }

        private async Task UpdateAuthenticatorConfig(AuthenticationFlowExecution execution, AuthenticationExecutionModel update, AuthenticatorConfig? config, KeycloakSeederConfigModel authenticatorConfig, CancellationToken cancellationToken)
        {
            switch (execution.AuthenticationConfig, update.AuthenticatorConfig)
            {
                case (null, null):
                    break;

                case (null, var _) when authenticatorConfig.ModificationAllowed(ModificationType.Create, update.AuthenticatorConfig):
                    await keycloak.CreateAuthenticationExecutionConfigurationAsync(
                        _realm,
                        execution.Id!,
                        new AuthenticatorConfig
                        {
                            Alias = update.AuthenticatorConfig,
                            Config = seedDataHandler.GetAuthenticatorConfig(update.AuthenticatorConfig).Config?.FilterNotNullValues().ToDictionary()
                        },
                        cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                    break;

                case (var _, null) when authenticatorConfig.ModificationAllowed(ModificationType.Delete, update.AuthenticatorConfig):
                    await keycloak.DeleteAuthenticatorConfigurationAsync(
                        _realm,
                        execution.AuthenticationConfig,
                        cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                    break;

                case var (_, _) when authenticatorConfig.ModificationAllowed(ModificationType.Update, update.AuthenticatorConfig):
                    var updateConfig = seedDataHandler.GetAuthenticatorConfig(update.AuthenticatorConfig);
                    if (config == null)
                        throw new UnexpectedConditionException("authenticatorConfig is null");
                    config.Alias = update.AuthenticatorConfig;
                    config.Config = updateConfig.Config?.FilterNotNullValues().ToDictionary();
                    await keycloak.UpdateAuthenticatorConfigurationAsync(_realm, execution.AuthenticationConfig!, config, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                    break;
            }
        }

        private bool CompareFlowExecutions(AuthenticationFlowExecution execution, AuthenticationExecutionModel update) =>
            execution.Description == seedDataHandler.GetAuthenticationFlow(update.FlowAlias).Description &&
            execution.DisplayName == update.FlowAlias &&
            execution.Requirement == update.Requirement;

        private Task<(bool IsEqual, AuthenticatorConfig? AuthenticatorConfig)> CompareExecutions(AuthenticationFlowExecution execution, AuthenticationExecutionModel update, CancellationToken cancellationToken) =>
            (execution.ProviderId != update.Authenticator ||
            execution.Requirement != update.Requirement)
                ? Task.FromResult<(bool, AuthenticatorConfig?)>((false, null))
                : ((execution.AuthenticationConfig, update.AuthenticatorConfig) switch
                {
                    (null, null) => Task.FromResult<(bool, AuthenticatorConfig?)>((true, null)),
                    (null, _) => Task.FromResult<(bool, AuthenticatorConfig?)>((false, null)),
                    (_, null) => Task.FromResult<(bool, AuthenticatorConfig?)>((false, null)),
                    (_, _) => CompareAuthenticationConfig(execution.AuthenticationConfig, update.AuthenticatorConfig, cancellationToken)
                });

        private async Task<(bool, AuthenticatorConfig?)> CompareAuthenticationConfig(string authenticatorConfigId, string authenticatorConfigAlias, CancellationToken cancellationToken)
        {
            var config = await keycloak.GetAuthenticatorConfigurationAsync(_realm, authenticatorConfigId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            var update = seedDataHandler.GetAuthenticatorConfig(authenticatorConfigAlias);
            return (CompareAuthenticatorConfig(config, update), config);
        }

        private static bool CompareAuthenticatorConfig(AuthenticatorConfig config, AuthenticatorConfigModel update) =>
            config.Alias == update.Alias &&
            config.Config.NullOrContentEqual(update.Config?.FilterNotNullValues());

        private Task<IEnumerable<AuthenticationFlowExecution>> GetExecutions(string alias, CancellationToken cancellationToken) =>
            keycloak.GetAuthenticationFlowExecutionsAsync(_realm, alias, cancellationToken);

        private IDictionary<string, object> CreateDataWithAliasTypeProviderDescription(AuthenticationExecutionModel execution)
        {
            var seedFlow = seedDataHandler.GetAuthenticationFlow(execution.FlowAlias);
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

        private sealed class ExecutionNode
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
}
