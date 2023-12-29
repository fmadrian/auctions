using System.Security.Claims;

namespace AuctionService.UnitTests;
public class Helpers
{
    // Method that will be used to pass as User object during testing.
    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        // Add claims you need.
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testUsername") };
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}
