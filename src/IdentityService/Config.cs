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
            },
            // NextJS application client.
            new Client(){
                ClientId = "auctionsNextApp",
                ClientName = "auctionsNextApp",
                ClientSecrets = {new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, // For a React Native app we would use Code.
                RequirePkce = false, // On a React Native app we would have to use PKCE.
                RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile", "auctionApp"},
                AccessTokenLifetime = 3600*24*30 // Default is 3600 (1 hour). Token not be longer than an hour (it can be in development).
            }
        };

}
