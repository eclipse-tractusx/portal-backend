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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

public static class SeedExtensions
{
    public static Action<PortalDbContext> SeedAddress(params Address[] additionalAddresses) => dbContext =>
    {
        dbContext.Addresses.AddRange(additionalAddresses);
    };

    public static Action<PortalDbContext> SeedOffers(params Offer[] additionalOffers) => dbContext =>
    {
        dbContext.Offers.AddRange(OfferData.Offers);
        dbContext.Offers.AddRange(additionalOffers);
    };

    public static Action<PortalDbContext> SeedAppInstances(params AppInstance[] additionalAppInstances) => dbContext =>
    {
        dbContext.AppInstances.AddRange(AppInstanceData.AppInstances);
        dbContext.AppInstances.AddRange(additionalAppInstances);
    };

    public static Action<PortalDbContext> SeedCompany(params Company[] additionalCompanies) => dbContext =>
    {
        dbContext.Companies.AddRange(additionalCompanies);
    };

    public static Action<PortalDbContext> SeedCompanyUser(params CompanyUser[] additionalUsers) => dbContext =>
    {
        dbContext.CompanyUsers.AddRange(additionalUsers);
    };

    public static Action<PortalDbContext> SeedIamUsers(params IamUser[] additionalUsers) => dbContext =>
    {
        dbContext.IamUsers.AddRange(additionalUsers);
    };

    public static Action<PortalDbContext> SeedIamClients(params IamClient[] additionalIamClients) =>
        dbContext =>
        {
            dbContext.IamClients.AddRange(additionalIamClients);
        };

    public static Action<PortalDbContext> SeedUserRoles(
        params UserRole[] additionalCompanyUserRoles) =>
        dbContext =>
        {
            dbContext.UserRoles.AddRange(additionalCompanyUserRoles);
        };

    public static Action<PortalDbContext> SeedCompanyUserAssignedRoles(
        params CompanyUserAssignedRole[] additionalCompanyUserRoles) =>
        dbContext =>
        {
            dbContext.CompanyUserAssignedRoles.AddRange(additionalCompanyUserRoles);
        };

    public static Action<PortalDbContext> SeedNotification(params Notification[] notifications) => dbContext =>
    {
        dbContext.Notifications.AddRange(notifications);
    };

    public static Action<PortalDbContext> SeedAgreementAssignedOffers(params AgreementAssignedOffer[] agreementAssignedOffers) => dbContext =>
    {
        dbContext.AgreementAssignedOffers.AddRange(agreementAssignedOffers);
    };

    public static Action<PortalDbContext> SeedOfferSubscriptions(params OfferSubscription[] offerSubscriptions) => dbContext =>
    {
        dbContext.OfferSubscriptions.AddRange(offerSubscriptions);
    };
}
