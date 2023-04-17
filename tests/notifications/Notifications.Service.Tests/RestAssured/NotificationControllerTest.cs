using Xunit;
using static RestAssured.Dsl;

namespace Notifications.Service.Tests.RestAssured;

public class NotificationControllerTests
{
    [Fact]
    public void NotificationCountDetails()
    {
        // Given
        Given()
            // .OAuth2("view_notifications")
            .Header(
                "Authorization",
                "Bearer " + "token")
            .When()
            .Get("http://localhost:5000/api/notification/count-details")
            .Then()
            .StatusCode(401);
    }
}