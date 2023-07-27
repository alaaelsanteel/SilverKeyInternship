using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EdgeDB;

namespace ContactDatabase.Pages;

public class IndexModel : PageModel
{
    private readonly EdgeDBClient _edgeDBClient;
    public List<Contact>Contacts { set; get; } = new List<Contact>();
    public IndexModel(EdgeDBClient edgeDBClient)
    {
        _edgeDBClient = edgeDBClient;
    }
    public async Task OnGetAsync()
    {
        Contacts = await GetContactAsync();
    }

    public async Task<IActionResult> OnPostAsync(Contact contact)
    {
        if (ModelState.IsValid)
        {
            await _edgeDBClient.ExecuteAsync(
                "INSERT Contact{ first_name := <str>$firstName, last_name := <str>$lastName, " +
                "email := <str>$email, title := <str>$title, description := <str>$description," +
                "date_of_birth := <datetime>$dateOfBirth, marriage_status := <bool>$marriageStatus }",
                new Dictionary<string, object>
                {
                {"firstName", contact.FirstName },
                {"lastName", contact.LastName },
                {"email", contact.Email},
                {"title", contact.Title },
                {"description", contact.Description },
                {"dateOfBirth", contact.DateOfBirth },
                {"marriageStatus", contact.IsMarried },
                });

            return RedirectToPage();
        }

        Contacts = await GetContactAsync();
        return Page();
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
}

