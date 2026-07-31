using System.Security.Claims;
using FileManager.Domain.Aggregates.UserAgg;
using Kootam.Authentication.Abstractions.Claims;

namespace FileManager.Application.Authentication;

public sealed class UserClaimsMapper : IUserClaimsMapper<User>
{
    public List<Claim> MapToClaims(User user) =>
    [
        new(KootamClaimTypes.UserId, user.Id.ToString()),
        new(KootamClaimTypes.Email, user.Email),
        new(KootamClaimTypes.Username, user.Email),
        new(KootamClaimTypes.LoginValid, "true"),
        new(ClaimTypes.GivenName, user.FirstName),
        new(ClaimTypes.Surname, user.LastName)
    ];
}
