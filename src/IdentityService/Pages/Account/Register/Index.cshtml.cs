using System.Security.Claims;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        // 1. Inject UserManager to get access to users.
        private readonly UserManager<ApplicationUser> _userManager;
        public Index(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }
        // 2. Bind properties.
        [BindProperty] // Allows us to bind the properties (attributes) from an object (Input) to the page's HTML
        public RegisterViewModel Input { get; set; }
        [BindProperty]
        public bool RegisterSuccess { get; set; } // Whether we could sign up or not.

        // OnGet(): What to do when the user gets to the page.
        // 3. Get returnUrl from query string.
        public IActionResult OnGet(string returnUrl) // IActionResult to return the page to the user.
        {
            // 3.1. Pass it to the RegisterViewModel
            this.Input = new RegisterViewModel()
            {
                ReturnUrl = returnUrl
            };

            return Page();
        }
        // How to handle the POST request (sending the form).
        public async Task<IActionResult> OnPost()
        {
            // If we click cancel, go back to Home page.
            if (Input.Button != "register") return Redirect("~/");
            if (ModelState.IsValid)
            {
                // Get information from input and create new user.
                var user = new ApplicationUser()
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true
                };
                IdentityResult result = await this._userManager.CreateAsync(user, Input.Password);
                // If we create the user, we add the claims.
                if (result.Succeeded)
                {
                    await this._userManager.AddClaimsAsync(
                       user,
                       new Claim[]{
                            new Claim(JwtClaimTypes.Name, Input.FullName)
                       }
                    );
                    this.RegisterSuccess = true; // Change the value of this binded property.
                }
            }
            return Page();
        }
    }
}
