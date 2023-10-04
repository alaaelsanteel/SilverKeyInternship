using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoginWithCookieAuthentication.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public bool IsLoggedIn { get; set; }
    public IActionResult OnGet()
    {
        if(User.Identity.IsAuthenticated)
        {
            IsLoggedIn = true;
        }
        return Page();
    }
    public async Task<IActionResult>OnPost()
    {
        if(ModelState.IsValid)
        {
            if(Username == "intern" && Password== "summer 2023 july")
            {
                var claims = new List<Claim>
                { 
                   new Claim(ClaimTypes.Name, Username), 
                     new Claim(ClaimTypes.Role, "User")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                //sign in
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                //successful login
                IsLoggedIn = true;

            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password");
            }
        }
        return Page();
    }
    public async Task<IActionResult> OnPostLogout()
    {
        //sign out
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        IsLoggedIn = false;
        return Page();
    }
}
