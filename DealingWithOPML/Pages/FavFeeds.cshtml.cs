using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using System.Net.Http;

namespace DealingWithOPML.Pages;

public class FavFeedsModel : PageModel
{
    public List<RssItem> FavoriteRssList { get; set; } = new();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; private set; } = 20;

    public void OnGet()
    {
        var favoriteFeedsCookies = Request.Cookies["FavoriteFeeds"];
        var favoriteFeeds = new Dictionary<string, RssItem>(); ;
        if (favoriteFeedsCookies != null)
        {
             favoriteFeeds = JsonSerializer.Deserialize<Dictionary<string, RssItem>>(favoriteFeedsCookies);     
        }
        FavoriteRssList = favoriteFeeds.Values
            .ToList();
    }
}
    
