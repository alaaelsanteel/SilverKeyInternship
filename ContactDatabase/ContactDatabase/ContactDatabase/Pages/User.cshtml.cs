using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EdgeDB;
namespace ContactDatabase.Pages;

public class UserModel : PageModel
{
    private readonly EdgeDBClient _edgeDBClient;
    public List<Contact> Contacts { set; get; } = new List<Contact>();
    public UserModel(EdgeDBClient edgeDBClient)
    {
        _edgeDBClient = edgeDBClient;
    }
    public async Task OnGetAsync()
    {
        Contacts = await GetContactAsync();
    }
    public async Task<List<Contact>> GetContactAsync()
    {
        var result = await _edgeDBClient.QueryAsync<Contact>("SELECT Contact {FirstName :=.first_name, LastName :=.last_name, " +
            "Email := .email, Title := .title, Description := .description, DateOfBirth := .date_of_birth, IsMarried := .marriage_status }");
        return result.ToList();
    }
}
