using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
    public class CustomProfileService : IProfileService
    {
        // 1. Inject the user manager.
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            // 1. Inject the user manager.
            this._userManager = userManager;
        }

        // 2. Here is where we add the additional information to the JWT.
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // context.Subject == User ID.
            ApplicationUser user = await this._userManager.GetUserAsync(context.Subject);
            // Get existent claims from the user.
            var currentClaims = await this._userManager.GetClaimsAsync(user);
            // Instantiate and add new claims
            var claims = new List<Claim>(){
                // Add the additional claims here.
                new Claim("username", user.UserName)
            };
            context.IssuedClaims.AddRange(claims); // Add new claims.
            // Add to the token the user's full name as a claim.
            context.IssuedClaims.Add(currentClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));

        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}