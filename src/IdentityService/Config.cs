using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full access.")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // 1. Create a new client
            new Client(){
                ClientId = "postman",
                ClientName = "Postman",
                // openid and profile (related to the client's information), are defined in the IdentityResources variable.
                AllowedScopes = {"openid", "profile", "auctionApp"},
                // Not necessary, Postman will not redirect us.
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = new Secret[]{new Secret("NotASecret".Sha256())},
                AllowedGrantTypes =  {GrantType.ResourceOwnerPassword}, // Authentication flow.
            }
        };
}
