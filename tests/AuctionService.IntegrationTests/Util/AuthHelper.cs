using System.Security.Claims;

namespace AuctionService.IntegrationTests.Util;
public class AuthHelper
{
    public static Dictionary<string, object> GetBearerForUser(string username)
    {
        // For any username passed, returns a claim 'Name'.
        return new Dictionary<string, object> { { ClaimTypes.Name, username } };
    }
}
