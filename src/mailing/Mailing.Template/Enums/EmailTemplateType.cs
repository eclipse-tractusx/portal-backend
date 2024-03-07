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

using Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Attributes;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Template.Enums
{
    /// <summary>
    /// Base email template types for sending html emails.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmailTemplateType
    {
        /// <summary>
        /// Invitation email template for users that get added to a registration process. Includes a custom message.
        /// </summary>
        [Path("additional_user_invitation_with_message.html")]
        AdditionalUserInvitationWithMessage,

        /// <summary>
        /// Invitation email template for users that get added to a registration process.
        /// </summary>
        [Path("additional_user_invitation.html")]
        AdditionalUserInvitation,

        /// <summary>
        /// Email template for Catena-X registration process invitations.
        /// </summary>
        [Path("cx_admin_invitation.html")]
        CxAdminInvitation,

        /// <summary>
        /// Email template for sending password assignments.
        /// </summary>
        [Path("password.html")]
        Password,

        /// <summary>
        /// Email template for declaring the next steps in the registration process.
        /// </summary>
        [Path("nextsteps.html")]
        NextSteps,

        /// <summary>
        /// Email template for welcoming new portal users.
        /// </summary>
        [Path("portal_newuser_welcome.html")]
        PortalNewUserWelcome,

        /// <summary>
        /// Email template for welcoming after registration process.
        /// </summary>
        [Path("portal_welcome_email.html")]
        PortalWelcomeEmail,

        /// <summary>
        /// Email template for decline registration process.
        /// </summary>
        [Path("registration_declined.html")]
        PortalRegistrationDecline,

        /// <summary>
        /// Email template for notifying app providers of subscription requests.
        /// </summary>
        [Path("appprovider_subscription_request.html")]
        AppSubscriptionRequest,

        /// <summary>
        /// Email template for notifying app providers of subscription activition.
        /// </summary>
        [Path("appprovider_subscription_activation.html")]
        AppSubscriptionActivation,

        /// <summary>
        /// Email template for notifying service providers of subscription requests.
        /// </summary>
        [Path("serviceprovider_subscription_request.html")]
        ServiceSubscriptionRequest,

        /// <summary>
        /// Email template for notifying requester of subscription activations.
        /// </summary>
        [Path("serviceprovider_subscription_activation.html")]
        ServiceSubscriptionActivation,

        /// <summary>
        /// Email template for notifying about decline of an app.
        /// </summary>
        [Path("app_request_decline.html")]
        AppRequestDecline,

        /// <summary>
        /// Email template for notifying about decline of an service.
        /// </summary>
        [Path("service_request_decline.html")]
        ServiceRequestDecline,

        /// <summary>
        /// Email template for new users being created on own idp
        /// </summary>
        [Path("new_user_account_externalIdP.html")]
        NewUserAccountOwnIdp,

        /// <summary>
        /// Email template for credential approvals
        /// </summary>
        [Path("verified_credential_approved.html")]
        CredentialApproval,

        /// <summary>
        /// Email template for credential rejection
        /// </summary>
        [Path("verified_credential_declined.html")]
        CredentialRejected,

        /// <summary>
        /// Email template for app release activation approval
        /// </summary>
        [Path("appactivation_offer_release_approval.html")]
        AppReleaseActivationApproval,

        /// <summary>
        /// Email template for service release activation approval
        /// </summary>
        [Path("serviceactivation_offer_release_approval.html")]
        ServiceReleaseActivationApproval,

        /// <summary>
        /// Email template for the welcome email of an osp registered company
        /// </summary>
        [Path("osp_welcome_email.html")]
        OspWelcomeMail,

        /// <summary>
        /// Email template for the welcome email of an osp registered company
        /// </summary>
        [Path("deactivate_managed_idp.html")]
        DeactivateManagedIdp,

        /// <summary>
        /// Email template for the welcome email of an osp registered company
        /// </summary>
        [Path("delete_managed_idp.html")]
        DeleteManagedIdp
    }
}
