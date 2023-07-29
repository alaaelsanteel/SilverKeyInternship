using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EdgeDB;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ContactDatabase.Pages;

public class AdminModel : PageModel
{
    private readonly EdgeDBClient _edgeDBClient;
    public List<Contact> Contacts { set; get; } = new List<Contact>();
    public AdminModel(EdgeDBClient edgeDBClient)
    {
        _edgeDBClient = edgeDBClient;
    }
    public async Task OnGetAsync()
    {
        Contacts = await GetContactAsync();
    }
    //public Contact NewContact { set; get; }
    public async Task<IActionResult> OnPostAsync(Contact contact)
    {
        if (ModelState.IsValid)
        {
            var passwordHasher = new PasswordHasher<string>();
            contact.Password = passwordHasher.HashPassword(null, contact.Password);
            await _edgeDBClient.ExecuteAsync(
                "INSERT Contact{ first_name := <str>$firstName, last_name := <str>$lastName, " +
                "email := <str>$email, title := <str>$title, description := <str>$description," +
                "date_of_birth := <datetime>$dateOfBirth, marriage_status := <bool>$marriageStatus," +
                "username := <str>$username, password :=<str>$password, role := <str>$role }",
                new Dictionary<string, object>
                {
                {"firstName", contact.FirstName },
                {"lastName", contact.LastName },
                {"email", contact.Email},
                {"title", contact.Title },
                {"description", contact.Description },
                {"dateOfBirth", contact.DateOfBirth },
                {"marriageStatus", contact.IsMarried },
                {"username", contact.Username },
                {"password", contact.Password},
                {"role", contact.Role },
                });

            return RedirectToPage();
        }

        else
        {
            return Page();
        }

    }

    public async Task<List<Contact>> GetContactAsync()
    {
        var result = await _edgeDBClient.QueryAsync<Contact>("SELECT Contact {FirstName :=.first_name, LastName :=.last_name, " +
            "Email := .email, Title := .title, Description := .description, DateOfBirth := .date_of_birth, IsMarried := .marriage_status }");
        return result.ToList();
    }

}
public class Contact
{
    [BindProperty]
    public string FirstName { get; set; } = string.Empty;
    [BindProperty]
    public string LastName { get; set; } = string.Empty;
    [BindProperty]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    public string Title { get; set; } = string.Empty;
    [BindProperty]
    public string? Description { get; set; }
    [BindProperty]
    public DateTime DateOfBirth { get; set; }
    [BindProperty]
    public bool IsMarried { get; set; }
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    [MinLength(8)]
    public string Password { get; set; } 
    [BindProperty]
    public string Role { get; set; } 

}
