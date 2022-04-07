using CatenaX.NetworkServices.Mailing.Template.Attributes;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Mailing.Template.Enums
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
        PortalNewUserWelcome
    }
}
