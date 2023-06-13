namespace Notifications.Service.Tests.RestAssured;

public record NotificationContent(
    string offerId, string coreOfferName, string username, string removedRoles, string addedRoles);