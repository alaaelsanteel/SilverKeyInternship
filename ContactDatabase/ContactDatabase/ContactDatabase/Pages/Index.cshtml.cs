using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EdgeDB;
using Microsoft.AspNetCore.Identity;

namespace ContactDatabase.Pages;

public class IndexModel : PageModel
{
    private readonly EdgeDBClient _edgeDBClient;
   
    public IndexModel(EdgeDBClient edgeDBClient)
    {
        _edgeDBClient = edgeDBClient;
    }
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public string ErrorMessage { get; private set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public void OnGet()
    {
    }
    public async Task<IActionResult>OnPostAsync()
    {
        var user = await GetUserAsync(Username);
        if (user == null)
        {
            ErrorMessage = "Invalid Username";
            return Page();
        }
        var passwordHasher = new PasswordHasher<string>();
        var passwordVerification = passwordHasher.VerifyHashedPassword(null, user.Password, Password);

        if(passwordVerification != PasswordVerificationResult.Success)
        {
            ErrorMessage = "Invalid Username";
            return Page();
        }
        if(user.Role == "User")
        {
            return RedirectToPage();
        }
        else
        {
            return RedirectToPage("/Admin");
        }
    }
   
    private async Task<Contact>GetUserAsync(string username)
    {
        var result = await _edgeDBClient.QueryAsync<Contact>(
            "SELECT Contact {FirstName :=.first_name, LastName :=.last_name, " +
            "Email := .email, Title := .title, Description := .description, DateOfBirth := .date_of_birth, IsMarried := .marriage_status," +
            " Role := .role, Password := .password } FILTER .username = <str>$username",
            new Dictionary<string, object> { { "username",username} });

        var user = result.FirstOrDefault();

        return user;
    }
   
   
}
