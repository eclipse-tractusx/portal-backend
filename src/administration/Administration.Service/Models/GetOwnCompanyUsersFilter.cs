public record GetOwnCompanyUsersFilter(
    Guid? CompanyUserId,
    string? UserEntityId,
    string? FirstName,
    string? LastName,
    string? Email
);
