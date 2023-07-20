using DealingWithOPML.Pages;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapPost("/toggle", async(HttpContext context) =>
{
    try
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        await antiforgery.ValidateRequestAsync(context);

        var link = context.Request.Form["link"];
        var title = context.Request.Form["title"];
        var pubDate = context.Request.Form["pubDate"];
        var description = context.Request.Form["description"];

        var favoriteFeedsJson = context.Request.Cookies["FavoriteFeeds"];

        Dictionary<string, RssItem> favoriteFeeds = new Dictionary<string, RssItem>();
        if (!string.IsNullOrEmpty(favoriteFeedsJson))
        {
            favoriteFeeds = JsonSerializer.Deserialize<Dictionary<string, RssItem>>(favoriteFeedsJson);
        }

        if (favoriteFeeds.ContainsKey(link))
        {
            RssItem feed = favoriteFeeds[link];
            favoriteFeeds.Remove(link);
            feed.IsFavorite = false;
            favoriteFeeds[link] = feed;
        }
        else
        {
            RssItem feed = new RssItem { Link = link, Title = title, IsFavorite = true, PubDate = pubDate, Description = description };
            favoriteFeeds.Add(link, feed);
        }
        context.Response.Cookies.Append("FavoriteFeeds", JsonSerializer.Serialize(favoriteFeeds));

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        context.Response.Redirect("/Error?error=" + ex.ToString());
    }
});

app.MapRazorPages();

app.Run();
