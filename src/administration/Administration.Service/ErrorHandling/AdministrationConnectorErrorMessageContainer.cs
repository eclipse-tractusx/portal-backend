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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;

public class AdministrationConnectorErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<AdministrationConnectorErrors, string> {
                { AdministrationConnectorErrors.CONNECTOR_NOT_FOUND, "connector {connectorId} does not exist" },
                { AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY,"company {companyId} is not provider of connector {connectorId}"},
                { AdministrationConnectorErrors.CONNECTOR_UNEXPECTED_NO_BPN_ASSIGNED, "provider company {companyId} has no businessPartnerNumber assigned" },
                { AdministrationConnectorErrors.CONNECTOR_UNEXPECTED_NO_DESCRIPTION, "provider company {companyId} has no self description document" },
                { AdministrationConnectorErrors.CONNECTOR_NOT_OFFERSUBSCRIPTION_EXIST,"OfferSubscription {subscriptionId} does not exist"},
                { AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY_OFFER,"Company is not the provider of the offer"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_OFFERSUBSCRIPTION_LINKED,"OfferSubscription is already linked to a connector"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_STATUS_ACTIVE_OR_PENDING,"The offer subscription must be either {offerSubscriptionStatusIdActive} or {offerSubscriptionStatusIdPending}"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_NO_DESCRIPTION,"provider company {CompanyId} has no self description document"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_SET_BPN,"The bpn of company {companyId} must be set"},
                { AdministrationConnectorErrors.CONNECTOR_ARGUMENT_LOCATION_NOT_EXIST,"Location {location} does not exist"},
                { AdministrationConnectorErrors.CONNECTOR_ARGUMENT_TECH_USER_NOT_ACTIVE,"Technical User {technicalUserId} is not assigned to company {companyId} or is not active"},
                { AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY_NOR_HOST,"company {companyId} is neither provider nor host-company of connector {connectorId}"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_DELETION_DECLINED,"Connector status does not match a deletion scenario. Deletion declined"},
                { AdministrationConnectorErrors.CONNECTOR_DELETION_FAILED_OFFER_SUBSCRIPTION,"Deletion Failed. Connector {connectorId} connected to an active offer subscription, {activeConnectorOfferSubscription}"},
                { AdministrationConnectorErrors.CONNECTOR_ARGUMENT_INCORRECT_BPN,"Incorrect BPN {bpns} attribute value"},
                { AdministrationConnectorErrors.CONNECTOR_NOT_EXIST,"Connector {externalId} does not exist"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_ALREADY_ASSIGNED,"Connector {externalId} already has a document assigned"},
                { AdministrationConnectorErrors.CONNECTOR_NOT_HOST_COMPANY,"Company {companyId} is not the connectors host company"},
                { AdministrationConnectorErrors.CONNECTOR_CONFLICT_INACTIVE_STATE,"Connector {connectorId} is in state {connectorStatusId}"}
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(AdministrationConnectorErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationConnectorErrors
{
    CONNECTOR_NOT_FOUND,
    CONNECTOR_NOT_PROVIDER_COMPANY,
    CONNECTOR_UNEXPECTED_NO_BPN_ASSIGNED,
    CONNECTOR_UNEXPECTED_NO_DESCRIPTION,
    CONNECTOR_NOT_OFFERSUBSCRIPTION_EXIST,
    CONNECTOR_NOT_PROVIDER_COMPANY_OFFER,
    CONNECTOR_CONFLICT_OFFERSUBSCRIPTION_LINKED,
    CONNECTOR_CONFLICT_STATUS_ACTIVE_OR_PENDING,
    CONNECTOR_CONFLICT_NO_DESCRIPTION,
    CONNECTOR_CONFLICT_SET_BPN,
    CONNECTOR_ARGUMENT_LOCATION_NOT_EXIST,
    CONNECTOR_ARGUMENT_TECH_USER_NOT_ACTIVE,
    CONNECTOR_NOT_PROVIDER_COMPANY_NOR_HOST,
    CONNECTOR_CONFLICT_DELETION_DECLINED,
    CONNECTOR_DELETION_FAILED_OFFER_SUBSCRIPTION,
    CONNECTOR_ARGUMENT_INCORRECT_BPN,
    CONNECTOR_NOT_EXIST,
    CONNECTOR_CONFLICT_ALREADY_ASSIGNED,
    CONNECTOR_NOT_HOST_COMPANY,
    CONNECTOR_CONFLICT_INACTIVE_STATE
}
